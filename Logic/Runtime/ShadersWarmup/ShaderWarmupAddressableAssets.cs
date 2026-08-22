using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.VFX;

namespace MisterGames.Logic.ShadersWarmup {

    /// <summary>
    /// Shaders and vfx assets loaded for warmup through Addressables.
    /// <para>
    /// Why. The warmup collection and the vfx asset list reference assets directly, so those assets are built
    /// into the player as a separate copy along with the bootstrap scene, while gameplay scenes load their own
    /// copy from a bundle. These are different objects with different GPU programs: warming up the player copy
    /// does not help the bundle one, and the log shows the same variant being uploaded to the driver a second
    /// time with another instance id. So before warmup the same assets are taken through Addressables, and
    /// exactly the objects the game renders with are warmed up.
    /// </para>
    /// <para>
    /// Matching is done by name: a catalog key (GUID) is known in the editor only, and at runtime a variant
    /// carries just a <see cref="Shader"/>. Shader names in a project are almost unique, and duplicates are
    /// counted and reported by the editor tool that collects the keys.
    /// </para>
    /// <para>
    /// VFX graph shaders are a separate story. A graph is compiled into sub asset shaders inside its own .vfx:
    /// they are not main assets, they have no catalog key and cannot be loaded by key (the .vfx location has
    /// type <see cref="VisualEffectAsset"/>, and the catalog filters locations by the requested type). They can
    /// still be warmed up: the sub shaders arrive in memory together with the .vfx loaded by key, and those are
    /// exactly the objects the game renders with. They are caught by the difference of
    /// <see cref="Resources.FindObjectsOfTypeAll{T}"/> before and after the vfx load, see <see cref="IndexVisualEffectShaders"/>.
    /// </para>
    /// <para>
    /// Handles are held until the service is destroyed: <see cref="Addressables.Release{TObject}"/> would unload
    /// the bundle and everything warmed up with it.
    /// </para>
    /// </summary>
    internal sealed class ShaderWarmupAddressableAssets : IDisposable {

        private readonly Dictionary<string, Shader> _shaders = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Shader> _visualEffectShaders = new(StringComparer.Ordinal);
        private readonly Dictionary<string, VisualEffectAsset> _visualEffectAssets = new(StringComparer.Ordinal);
        private readonly List<AsyncOperationHandle> _handles = new();

        public int ShaderCount => _shaders.Count;
        public int VisualEffectShaderCount => _visualEffectShaders.Count;
        public int VisualEffectAssetCount => _visualEffectAssets.Count;

        public async UniTask Load(string[] shaderKeys, string[] visualEffectAssetKeys, CancellationToken ct) {
            await LoadAssets(shaderKeys, _shaders, ct);

            // The snapshot is taken right before the vfx load, so that only their sub shaders end up in the difference.
            var loadedBeforeVisualEffects = new HashSet<Shader>(Resources.FindObjectsOfTypeAll<Shader>());

            await LoadAssets(visualEffectAssetKeys, _visualEffectAssets, ct);

            int duplicateNames = IndexVisualEffectShaders(loadedBeforeVisualEffects);

            Debug.Log($"{nameof(ShaderWarmupAddressableAssets)}.{nameof(Load)}: f {Time.frameCount}, " +
                      $"shaders {_shaders.Count}/{shaderKeys?.Length ?? 0}, " +
                      $"visual effect assets {_visualEffectAssets.Count}/{visualEffectAssetKeys?.Length ?? 0}, " +
                      $"shaders arrived with visual effect bundles {_visualEffectShaders.Count}" +
                      (duplicateNames > 0 ? $", duplicate names among them {duplicateNames}" : string.Empty));
        }

        public void Dispose() {
            for (int i = 0; i < _handles.Count; i++) {
                if (_handles[i].IsValid()) Addressables.Release(_handles[i]);
            }

            _handles.Clear();
            _shaders.Clear();
            _visualEffectShaders.Clear();
            _visualEffectAssets.Clear();
        }

        /// <summary>
        /// A bundle copy of the shader if it is loaded, otherwise the source shader. Shaders loaded by their own
        /// keys are asked first: they are addressed explicitly and therefore more reliable than sub shaders
        /// caught by the difference of snapshots.
        /// </summary>
        public Shader ResolveShader(Shader shader) {
            if (shader == null) return null;

            if (_shaders.TryGetValue(shader.name, out var addressableShader) && addressableShader != null) {
                return addressableShader;
            }

            return _visualEffectShaders.TryGetValue(shader.name, out var visualEffectShader) && visualEffectShader != null
                ? visualEffectShader
                : shader;
        }

        /// <summary>
        /// The same vfx asset list, with bundle copies substituted where they exist.
        /// The source array belongs to the settings and is not modified.
        /// </summary>
        public VisualEffectAsset[] ResolveVisualEffectAssets(VisualEffectAsset[] visualEffectAssets) {
            if (visualEffectAssets == null || visualEffectAssets.Length == 0) return Array.Empty<VisualEffectAsset>();
            if (_visualEffectAssets.Count == 0) return visualEffectAssets;

            var resolved = new VisualEffectAsset[visualEffectAssets.Length];

            for (int i = 0; i < visualEffectAssets.Length; i++) {
                var visualEffectAsset = visualEffectAssets[i];

                resolved[i] = visualEffectAsset != null
                              && _visualEffectAssets.TryGetValue(visualEffectAsset.name, out var addressableAsset)
                              && addressableAsset != null
                    ? addressableAsset
                    : visualEffectAsset;
            }

            return resolved;
        }

        /// <summary>
        /// Indexes shaders that arrived with vfx bundles, first of all the sub shaders of the graphs themselves
        /// (Hidden/VFX/[Asset]/[System]/[Output]), for which no catalog key exists.
        /// Returns how many names occurred in the difference more than once.
        /// <para>
        /// Shaders already taken by their own keys are not overridden: those are addressed explicitly, and the
        /// difference of snapshots knows nothing better about them. Everything else new goes into the index,
        /// these are bundle copies, that is exactly the objects the game renders with. If a bundle loaded by
        /// someone else falls into the load window, its shaders are indexed as well, which does no harm
        /// for the same reason.
        /// </para>
        /// </summary>
        private int IndexVisualEffectShaders(HashSet<Shader> loadedBeforeVisualEffects) {
            var shaders = Resources.FindObjectsOfTypeAll<Shader>();
            int duplicateNames = 0;

            for (int i = 0; i < shaders.Length; i++) {
                var shader = shaders[i];

                if (shader == null || string.IsNullOrEmpty(shader.name)) continue;
                if (loadedBeforeVisualEffects.Contains(shader)) continue;
                if (_shaders.ContainsKey(shader.name)) continue;

                if (!_visualEffectShaders.TryAdd(shader.name, shader)) duplicateNames++;
            }

            return duplicateNames;
        }

        private async UniTask LoadAssets<T>(string[] keys, Dictionary<string, T> result, CancellationToken ct)
            where T : UnityEngine.Object
        {
            if (keys == null || keys.Length == 0) return;

            AsyncOperationHandle<IList<T>> handle;

            try {
                // Union: keys that are not in the catalog are simply skipped, groups could have been rebuilt
                // after the keys were collected.
                // releaseDependenciesOnFailure: false, one failed load should not drop the rest.
                handle = Addressables.LoadAssetsAsync<T>(keys, null, Addressables.MergeMode.Union, false);
            }
            catch (Exception exception) {
                Debug.LogWarning($"{nameof(ShaderWarmupAddressableAssets)}: cannot start loading " +
                                 $"{typeof(T).Name} through Addressables ({keys.Length} keys): {exception.Message}. " +
                                 $"Warmup goes by direct references.");
                return;
            }

            if (!handle.IsValid()) return;

            _handles.Add(handle);

            try {
                await handle.ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException) {
                throw;
            }
            catch (Exception exception) {
                Debug.LogWarning($"{nameof(ShaderWarmupAddressableAssets)}: loading {typeof(T).Name} " +
                                 $"through Addressables failed: {exception.Message}. " +
                                 $"Warmup goes by direct references.");
                return;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null) {
                Debug.LogWarning($"{nameof(ShaderWarmupAddressableAssets)}: Addressables returned no " +
                                 $"{typeof(T).Name} for {keys.Length} keys. Warmup goes by direct references.");
                return;
            }

            foreach (var asset in handle.Result) {
                if (asset != null) result[asset.name] = asset;
            }
        }
    }

}
