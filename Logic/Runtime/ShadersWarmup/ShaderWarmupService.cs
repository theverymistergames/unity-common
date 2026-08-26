using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Tick;
using MisterGames.Scenes.Loading;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace MisterGames.Logic.ShadersWarmup {

    [DefaultExecutionOrder(-100_000)]
    internal sealed class ShaderWarmupService : MonoBehaviour, IUpdate {

        [SerializeField] private ShaderWarmupSettings _settings;

        public event Action<float> OnWarmupProgress = delegate { };
        public event Action OnWarmupStarted = delegate { };
        public event Action OnWarmupCompleted = delegate { };
        public bool IsWarmupCompleted { get; private set; }

        public static ShaderWarmupService Instance { get; private set; }

        /// <summary>
        /// Estimated permutation count per variant. WarmUpProgressively spends its budget not on variants,
        /// but on permutations, the stages of a variant: most have two of them (vertex + pixel), tessellation
        /// passes have up to six. Hence the starting warmup budget and the progress denominator,
        /// the budget grows by itself while the engine reports progress.
        /// </summary>
        private const int EstimatedPermutationsPerVariant = 2;

        /// <summary>
        /// A hard budget cap, an insurance against an endless loop if the engine reports nothing
        /// through isWarmedUp nor through completedWarmupCount.
        /// </summary>
        private const int MaxPermutationsPerVariant = 6;

        /// <summary>
        /// How many iterations in a row the warmed up counter is allowed to stand still before deciding
        /// there is nothing left to warm up. The margin is needed in case the engine does not count
        /// on every iteration.
        /// </summary>
        private const int IdleIterationsBeforeStop = 8;

        /// <summary> How many variants to process before yielding a frame while rebinding the collection to Addressables. </summary>
        private const int RebindYieldPeriod = 256;

        private CancellationTokenSource _cts;
        private GraphicsStateCollection _graphicsStateCollection;
        private ShaderWarmupAddressableAssets _addressableAssets;
        private bool _isTracing;
        private float _saveTimer;
        private bool _shaderWarmupScenePassCompleted;

        private void Awake() {
            Instance = this;
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _cts);
            PlayerLoopStage.LateUpdate.Unsubscribe(this);

            _addressableAssets?.Dispose();
            _addressableAssets = null;

#if UNITY_EDITOR
            return;
#endif

#if DEVELOPMENT_BUILD
            UnloadForDevelopmentBuild();
            return;
#endif
        }

        public ShaderWarmupSettings GetSettings() {
            return _settings;
        }

        public async UniTask Load() {
            AsyncExt.RecreateCts(ref _cts);

#if UNITY_EDITOR
            await LoadForEditor(_cts.Token);
            return;
#endif

#if DEVELOPMENT_BUILD
            await LoadForDevelopmentBuild(_cts.Token);
            return;
#endif

            await LoadForReleaseBuild(_cts.Token);
        }

        public void NotifyShaderWarmupScenePassCompleted() {
            _shaderWarmupScenePassCompleted = true;
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_isTracing) return;

            _saveTimer += dt;
            if (_saveTimer < _settings.SavePeriod) return;

            _saveTimer = 0f;
            EndTracingAndSaveToLocalFile();
            BeginTracing();
        }

        private async UniTask LoadForEditor(CancellationToken cancellationToken) {
            if (_settings.EnterShadersWarmupSceneOnBootstrapInEditor) {
                if (IsSceneIncludedInBuildList(_settings.WarmupSceneName)) {
                    Debug.LogWarning($"ShaderWarmupService.LoadForEditor: f {Time.frameCount}, " +
                                     $"open shaders warmup scene {_settings.WarmupSceneName} for testing. " +
                                     $"It can be disabled in {_settings}.");

                    await Fader.Main.FadeOutAsync(0f);

                    await SceneManager.LoadSceneAsync(_settings.WarmupSceneName, LoadSceneMode.Additive);

                    while (!_shaderWarmupScenePassCompleted && !cancellationToken.IsCancellationRequested) {
                        await UniTask.Yield();
                    }

                    if (cancellationToken.IsCancellationRequested) return;

                    await SceneManager.UnloadSceneAsync(_settings.WarmupSceneName);

                    await Fader.Main.FadeInAsync(0f);
                }
                else {
                    Debug.LogWarning($"ShaderWarmupService.LoadForEditor: f {Time.frameCount}, " +
                                     $"tried to open shaders warmup scene {_settings.WarmupSceneName} for testing, " +
                                     $"but it is not included in build scene list. Skip.");
                }
            }

            NotifyWarmupStarted();

            if (_settings.SimulateShadersWarmupInEditorDuration > 0f)
            {
                Debug.LogWarning($"ShaderWarmupService.LoadForEditor: f {Time.frameCount}, " +
                                 $"simulate shaders warmup for testing. " +
                                 $"It can be disabled in {_settings}.");

                float t = 0f;
                float speed = 1f / _settings.SimulateShadersWarmupInEditorDuration;
                while (t < 1f && !cancellationToken.IsCancellationRequested) {
                    t = Mathf.Clamp01(t + TimeSources.unscaledDeltaTime * speed);
                    NotifyWarmupProgress(t);
                    await UniTask.Yield();
                }

                if (cancellationToken.IsCancellationRequested) return;
            }

            NotifyWarmupCompleted();

            if (_settings.SimulateShadersWarmupInEditorDuration > 0f) {
                await AsyncExt.DelayUnscaled(_settings.DisableViewDelay + _settings.DisableViewFader, cancellationToken);
            }
        }

        private static bool IsSceneIncludedInBuildList(string sceneName) {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                string name = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                if (string.Equals(name, sceneName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private async UniTask LoadForDevelopmentBuild(CancellationToken cancellationToken) {
            _graphicsStateCollection = CreateCollectionForCurrentDevice();
            SaveToLocalFile(_graphicsStateCollection); // clear before tracing
            BeginTracing();

            if (_settings.EnterShadersWarmupSceneOnBootstrapInDevBuild) {
                if (IsSceneIncludedInBuildList(_settings.WarmupSceneName)) {
                    Debug.LogWarning($"ShaderWarmupService.LoadForDevelopmentBuild: f {Time.frameCount}, " +
                                     $"open shaders warmup scene {_settings.WarmupSceneName} for dev build tracing. " +
                                     $"It can be disabled in {_settings}.");

                    await Fader.Main.FadeOutAsync(0f);

                    await SceneManager.LoadSceneAsync(_settings.WarmupSceneName, LoadSceneMode.Additive);

                    while (!_shaderWarmupScenePassCompleted && !cancellationToken.IsCancellationRequested) {
                        await UniTask.Yield();
                    }

                    if (cancellationToken.IsCancellationRequested) return;

                    await SceneManager.UnloadSceneAsync(_settings.WarmupSceneName);

                    await Fader.Main.FadeInAsync(0f);
                }
                else {
                    Debug.LogWarning($"ShaderWarmupService.LoadForDevelopmentBuild: f {Time.frameCount}, " +
                                     $"tried to open shaders warmup scene {_settings.WarmupSceneName} for dev build tracing, " +
                                     $"but it is not included in build scene list. Skip.");
                }
            }

            EndTracingAndSaveToLocalFile();

            await StartWarmup(LoadReleaseCollection(), _settings.VisualEffectAssets, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;

            BeginTracing();
        }

        private void UnloadForDevelopmentBuild() {
            EndTracingAndSaveToLocalFile();
        }

        private UniTask LoadForReleaseBuild(CancellationToken ct) {
            return StartWarmup(LoadReleaseCollection(), _settings.VisualEffectAssets, ct);
        }

        private GraphicsStateCollection LoadLocalTracingCollection() {
            var graphicsStateCollection = new GraphicsStateCollection();

            string filePath = _settings.GetTracingFilePath();
            if (File.Exists(filePath)) graphicsStateCollection.LoadFromFile(filePath);

            // The file could be written by an older version of the service, without metadata: stamp it again,
            // otherwise the metadata travels into the release collection and warmup complains about a mismatch.
            StampCurrentDevice(graphicsStateCollection);

            return graphicsStateCollection;
        }

        /// <summary>
        /// A new collection marked with the current device, platform and quality level.
        /// Without the marks the engine considers the collection captured elsewhere and prints
        /// three mismatch warnings on every warmup batch.
        /// </summary>
        private static GraphicsStateCollection CreateCollectionForCurrentDevice() {
            var graphicsStateCollection = new GraphicsStateCollection();
            StampCurrentDevice(graphicsStateCollection);

            return graphicsStateCollection;
        }

        private static void StampCurrentDevice(GraphicsStateCollection graphicsStateCollection) {
            graphicsStateCollection.graphicsDeviceType = SystemInfo.graphicsDeviceType;
            graphicsStateCollection.runtimePlatform = Application.platform;
            graphicsStateCollection.qualityLevelName = GetCurrentQualityLevelName();
        }

        private static string GetCurrentQualityLevelName() {
            string[] names = QualitySettings.names;
            int level = QualitySettings.GetQualityLevel();

            return names != null && level >= 0 && level < names.Length ? names[level] : string.Empty;
        }

        private void BeginTracing() {
            _isTracing = _graphicsStateCollection.BeginTrace();
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void EndTracingAndSaveToLocalFile() {
            _isTracing = false;
            _graphicsStateCollection.EndTrace();
            PlayerLoopStage.LateUpdate.Unsubscribe(this);

            var localTracingCollection = LoadLocalTracingCollection();
            localTracingCollection.Append(_graphicsStateCollection);
            SaveToLocalFile(localTracingCollection);
        }

        private void SaveToLocalFile(GraphicsStateCollection collection) {
            string filePath = _settings.GetTracingFilePath();
            string directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            if (!File.Exists(filePath))
                using (File.Create(filePath)) { }

            StampCurrentDevice(collection);
            collection.SaveToFile(filePath);
        }

        private GraphicsStateCollection LoadReleaseCollection() {
            return _settings.GetReleaseGraphicsStateCollection();
        }

        private async UniTask StartWarmup(
            GraphicsStateCollection graphicsStateCollection,
            VisualEffectAsset[] visualEffectAssets,
            CancellationToken ct)
        {
            NotifyWarmupStarted();

            try {
                await WarmupRoutine(graphicsStateCollection, visualEffectAssets, ct);
            }
            catch (OperationCanceledException) {
                //
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
            finally {
                NotifyWarmupCompleted();
            }
        }

        private async UniTask WarmupRoutine(
            GraphicsStateCollection graphicsStateCollection,
            VisualEffectAsset[] visualEffectAssets,
            CancellationToken ct)
        {
            if (_settings.WarmupStartDelayFrames > 0) {
                await UniTask.DelayFrame(_settings.WarmupStartDelayFrames, cancellationToken: ct);
            }

            if (_settings.UseAddressablesForWarmup) {
                // A repeated warmup must not leak the handles of the previous one.
                _addressableAssets?.Dispose();
                _addressableAssets = new ShaderWarmupAddressableAssets();

                await _addressableAssets.Load(
                    _settings.AddressableShaderKeys,
                    _settings.AddressableVisualEffectKeys,
                    ct);

                graphicsStateCollection = await RebindToAddressableShaders(graphicsStateCollection, ct);
                visualEffectAssets = _addressableAssets.ResolveVisualEffectAssets(visualEffectAssets);
            }

            int totalVariants = GetVariantCount(graphicsStateCollection);
            int totalVisualEffects = visualEffectAssets.Length;
            int total = totalVariants + totalVisualEffects;

            // Both phases split the progress bar proportionally to the amount of work: the pso phase
            // no longer knows in advance how many steps it has left.
            float graphicsStatesShare = total > 0 ? (float) totalVariants / total : 1f;

            await WarmupGraphicsStates(graphicsStateCollection, 0f, graphicsStatesShare, ct);
            await WarmupVisualEffects(visualEffectAssets, graphicsStatesShare, 1f, ct);
        }

        /// <summary>
        /// Runs warmup until the engine reports the collection is warmed up entirely.
        /// <para>
        /// The loop used to count steps by itself and stop after variantCount of them, while
        /// WarmUpProgressively spends its budget on permutations (stages of a variant), and there are about
        /// twice as many. Half of the collection was simply never reached.
        /// </para>
        /// <para>
        /// The exit condition now comes from the engine: isWarmedUp or a stalled completedWarmupCount.
        /// The budget estimation stays as an insurance in case the counters are not maintained on the platform,
        /// then the counter stands at zero and cannot be trusted.
        /// </para>
        /// </summary>
        private async UniTask WarmupGraphicsStates(
            GraphicsStateCollection graphicsStateCollection,
            float progressFrom,
            float progressTo,
            CancellationToken ct)
        {
            int variantCount = GetVariantCount(graphicsStateCollection);

            if (variantCount == 0) {
                NotifyWarmupProgress(progressTo);
                return;
            }

            int batch = _settings.ProgressiveWarmupBatchCountPso;
            int budget = variantCount * EstimatedPermutationsPerVariant;
            int hardLimit = variantCount * MaxPermutationsPerVariant;
            int estimatedTotal = budget;

            var handle = new JobHandle();
            int requested = 0;
            int completed = 0;
            int idleIterations = 0;

            while (!ct.IsCancellationRequested && requested < budget && requested < hardLimit) {
                handle = graphicsStateCollection.WarmUpProgressively(batch, handle);
                handle.Complete();

                requested += batch;

                if (graphicsStateCollection.isWarmedUp) break;

                int completedNow = graphicsStateCollection.completedWarmupCount;

                if (completedNow > 0) {
                    // The counter is alive: keep going while warmup moves, not while the estimation lasts.
                    budget = Mathf.Max(budget, completedNow + batch);

                    if (completedNow == completed) {
                        if (++idleIterations >= IdleIterationsBeforeStop) break;
                    }
                    else {
                        idleIterations = 0;
                    }
                }

                completed = completedNow;
                estimatedTotal = Mathf.Max(estimatedTotal, completed + batch);

                int progressDone = completed > 0 ? completed : requested;
                int progressTotal = completed > 0 ? estimatedTotal : budget;

                NotifyWarmupProgress(Mathf.Lerp(progressFrom, progressTo, (float) progressDone / progressTotal));

                await UniTask.Yield();
            }

            Debug.Log($"ShaderWarmupService.WarmupGraphicsStates: f {Time.frameCount}, " +
                      $"variants {variantCount}, permutations requested {requested}, " +
                      $"engine completed {graphicsStateCollection.completedWarmupCount}, " +
                      $"warmed up {graphicsStateCollection.isWarmedUp}");

            NotifyWarmupProgress(progressTo);
        }

        private async UniTask WarmupVisualEffects(
            VisualEffectAsset[] visualEffectAssets,
            float progressFrom,
            float progressTo,
            CancellationToken ct)
        {
            int totalVisualEffects = visualEffectAssets.Length;

            if (totalVisualEffects == 0) {
                NotifyWarmupProgress(progressTo);
                return;
            }

            int warmedVisualEffects = 0;
            int batchVisualEffects = _settings.ProgressiveWarmupBatchCountVisualEffectAssets;

            while (!ct.IsCancellationRequested && warmedVisualEffects < totalVisualEffects) {
                int batch = Mathf.Min(batchVisualEffects, totalVisualEffects - warmedVisualEffects);

                for (int i = 0; i < batch; i++) {
                    var visualEffectAsset = visualEffectAssets[warmedVisualEffects + i];
                    if (visualEffectAsset != null) visualEffectAsset.PrewarmComputeShaders();
                }

                warmedVisualEffects += batch;

                NotifyWarmupProgress(Mathf.Lerp(progressFrom, progressTo, (float) warmedVisualEffects / totalVisualEffects));

                await UniTask.Yield();
            }

            NotifyWarmupProgress(progressTo);
        }

        /// <summary>
        /// Rebuilds the collection so that its variants point to shaders loaded through Addressables, that is
        /// to the same objects the game renders with later. Variants that have no addressable copy are moved as is.
        /// <para>
        /// Sub shaders of vfx graphs land here as well: they have no catalog key, but they arrive in memory
        /// together with their .vfx, and <see cref="ShaderWarmupAddressableAssets.ResolveShader"/> finds them.
        /// Without it their variants were warmed up on the copy built into the player by a direct collection
        /// reference, while the game rendered with the bundle copy and compiled them again in a frame.
        /// </para>
        /// </summary>
        private async UniTask<GraphicsStateCollection> RebindToAddressableShaders(
            GraphicsStateCollection source,
            CancellationToken ct)
        {
            if (_addressableAssets.ShaderCount == 0 && _addressableAssets.VisualEffectShaderCount == 0) return source;

            var variants = ListPool<GraphicsStateCollection.ShaderVariant>.Get();
            var keywords = new List<LocalKeyword>();

            // On D3D11 there are no graphics states in the collection at all (there are no PSOs there, the engine
            // falls back to warming up variants), so there is nothing to carry. On DX12 and Vulkan there are,
            // and losing them on rebind would throw away the whole point of the collection.
            var graphicsStates = source.totalGraphicsStateCount > 0
                ? new List<GraphicsStateCollection.GraphicsState>()
                : null;

            try {
                source.GetVariants(variants);

                var rebound = CreateCollectionForCurrentDevice();

                int reboundVariants = 0;
                int droppedKeywords = 0;

                for (int i = 0; i < variants.Count; i++) {
                    var variant = variants[i];

                    if (variant.shader == null) continue;

                    var shader = _addressableAssets.ResolveShader(variant.shader);
                    var variantKeywords = variant.keywords ?? Array.Empty<LocalKeyword>();

                    if (shader != variant.shader) {
                        droppedKeywords += RemapKeywords(shader, variant.keywords, keywords);
                        variantKeywords = keywords.ToArray();
                        reboundVariants++;
                    }

                    rebound.AddVariant(shader, variant.passId, variantKeywords);

                    if (graphicsStates != null) {
                        CopyGraphicsStates(source, variant, rebound, shader, variant.passId, variantKeywords, graphicsStates);
                    }

                    if ((i + 1) % RebindYieldPeriod == 0) await UniTask.Yield(ct);
                }

                Debug.Log($"ShaderWarmupService.RebindToAddressableShaders: f {Time.frameCount}, " +
                          $"variants {variants.Count}, rebound to addressable shaders {reboundVariants}, " +
                          $"addressable shaders loaded {_addressableAssets.ShaderCount}, " +
                          $"visual effect sub-shaders indexed {_addressableAssets.VisualEffectShaderCount}, " +
                          $"keywords dropped {droppedKeywords}");

                return rebound;
            }
            finally {
                ListPool<GraphicsStateCollection.ShaderVariant>.Release(variants);
            }
        }

        /// <summary> Moves graphics states of a variant into the target collection, onto its variant. </summary>
        private static void CopyGraphicsStates(
            GraphicsStateCollection source,
            GraphicsStateCollection.ShaderVariant sourceVariant,
            GraphicsStateCollection target,
            Shader shader,
            PassIdentifier passId,
            LocalKeyword[] keywords,
            List<GraphicsStateCollection.GraphicsState> buffer)
        {
            buffer.Clear();
            source.GetGraphicsStatesForVariant(sourceVariant, buffer);

            for (int i = 0; i < buffer.Count; i++) {
                target.AddGraphicsStateForVariant(shader, passId, keywords, buffer[i]);
            }
        }

        /// <summary>
        /// Moves keywords of a variant into the keyword space of another shader: a <see cref="LocalKeyword"/>
        /// is bound to its own shader, and a foreign one is not accepted by the collection.
        /// Returns the count of dropped keywords.
        /// </summary>
        private static int RemapKeywords(Shader shader, LocalKeyword[] source, List<LocalKeyword> result) {
            result.Clear();

            if (source == null || source.Length == 0) return 0;

            var keywordSpace = shader.keywordSpace;
            int dropped = 0;

            for (int i = 0; i < source.Length; i++) {
                var keyword = keywordSpace.FindKeyword(source[i].name);

                if (keyword.isValid) result.Add(keyword);
                else dropped++;
            }

            return dropped;
        }

        private static int GetVariantCount(GraphicsStateCollection graphicsStateCollection)
        {
            var variants = ListPool<GraphicsStateCollection.ShaderVariant>.Get();

            try
            {
                graphicsStateCollection.GetVariants(variants);
                return variants.Count;
            }
            finally
            {
                ListPool<GraphicsStateCollection.ShaderVariant>.Release(variants);
            }
        }

        private void NotifyWarmupStarted() {
            OnWarmupStarted.Invoke();
        }

        private void NotifyWarmupProgress(float progress) {
            OnWarmupProgress.Invoke(progress);
        }

        private void NotifyWarmupCompleted() {
            IsWarmupCompleted = true;
            OnWarmupCompleted.Invoke();
        }
    }

}
