using System;
using System.Collections.Generic;
using MisterGames.Logic.ShadersWarmup;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace MisterGames.Logic.Editor.ShadersWarmup {

    /// <summary>
    /// Collects Addressables keys for the shaders of the release collection and for the warmup vfx assets.
    /// <para>
    /// A key is a GUID: it is in the catalog for every project group (Include GUID In Catalog) and, unlike
    /// an address, it survives renaming. At runtime the loaded assets are matched with variants by name,
    /// so equal names of different assets are counted separately, only one of such a pair gets warmed up.
    /// </para>
    /// <para>
    /// Assets without an entry in the groups stay on direct references: there is nothing to get their bundle
    /// copy with. To cover those, a shader or a vfx asset needs its own Addressables entry, which changes the
    /// bundle layout, so this tool does not do it and only counts them.
    /// See <see cref="ShaderWarmupAddressableFixer"/> for the tool that does create the entries.
    /// </para>
    /// <para>
    /// The tool lives in the editor assembly because the runtime assembly cannot reference the Addressables
    /// editor assembly, and writes the result back through <c>ShaderWarmupSettings.SetAddressableKeys</c>.
    /// </para>
    /// </summary>
    internal static class ShaderWarmupAddressableKeys {

        [MenuItem(ShaderWarmupEditorUtils.MenuRoot + "Collect addressable keys of shaders and vfx assets", priority = 120)]
        private static void CollectMenuItem() {
            if (!ShaderWarmupEditorUtils.TryFindSettings(out var settings)) return;

            Collect(settings);
        }

        public static void Collect(ShaderWarmupSettings settings) {
            if (settings == null) return;

            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

            if (addressableSettings == null) {
                Debug.LogWarning($"{nameof(ShaderWarmupAddressableKeys)}: Addressables settings are not found. " +
                                 $"Keys are not collected, warmup goes by direct references.");
                return;
            }

            var shaderKeys = new List<string>();
            var missingShaders = new HashSet<string>(StringComparer.Ordinal);
            var subAssetShaders = new HashSet<string>(StringComparer.Ordinal);
            var duplicateNames = new HashSet<string>(StringComparer.Ordinal);
            var visitedNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var shader in settings.CollectReleaseCollectionShaders()) {
                if (!AssetDatabase.IsMainAsset(shader)) {
                    // Shaders of vfx graphs are sub assets inside a .vfx: they have no entry of their own and
                    // cannot be loaded as a Shader by key. At runtime they are caught by
                    // ShaderWarmupAddressableAssets anyway, they arrive in memory together with their .vfx.
                    subAssetShaders.Add(shader.name);
                    continue;
                }

                if (!TryGetAddressableKey(addressableSettings, shader, out string key)) {
                    missingShaders.Add(shader.name);
                    continue;
                }

                if (!visitedNames.Add(shader.name)) duplicateNames.Add(shader.name);

                shaderKeys.Add(key);
            }

            var visualEffectKeys = new List<string>();
            var missingVisualEffects = new HashSet<string>(StringComparer.Ordinal);

            var visualEffectAssets = settings.VisualEffectAssets;

            for (int i = 0; i < visualEffectAssets.Length; i++) {
                var visualEffectAsset = visualEffectAssets[i];
                if (visualEffectAsset == null) continue;

                if (TryGetAddressableKey(addressableSettings, visualEffectAsset, out string key)) visualEffectKeys.Add(key);
                else missingVisualEffects.Add(visualEffectAsset.name);
            }

            settings.SetAddressableKeys(shaderKeys.ToArray(), visualEffectKeys.ToArray());

            string report = $"{nameof(ShaderWarmupAddressableKeys)}: collected addressable keys, " +
                            $"shaders {shaderKeys.Count}, vfx assets {visualEffectKeys.Count}.";

            if (missingShaders.Count > 0) {
                report += $" Shaders with no entry in Addressables: {missingShaders.Count} " +
                          $"({ShaderWarmupEditorUtils.FormatSample(missingShaders)}) - they are warmed up by a direct " +
                          $"reference, that is not the copy the game renders with.";
            }

            if (missingVisualEffects.Count > 0) {
                report += $" Vfx assets with no entry in Addressables: {missingVisualEffects.Count} " +
                          $"({ShaderWarmupEditorUtils.FormatSample(missingVisualEffects)}).";
            }

            if (subAssetShaders.Count > 0) {
                report += $" Sub asset shaders (vfx graphs), for which no key exists: " +
                          $"{subAssetShaders.Count} ({ShaderWarmupEditorUtils.FormatSample(subAssetShaders)}).";
            }

            if (duplicateNames.Count > 0) {
                report += $" Names behind which there is more than one asset: {duplicateNames.Count} " +
                          $"({ShaderWarmupEditorUtils.FormatSample(duplicateNames)}) - only one of them is warmed up at runtime.";
            }

            Debug.Log(report);
        }

        private static bool TryGetAddressableKey(AddressableAssetSettings addressableSettings, UnityEngine.Object asset, out string key) {
            key = null;

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long _)) return false;
            if (string.IsNullOrEmpty(guid)) return false;

            // includeImplicit: an asset can have no entry of its own but lie inside an addressable folder,
            // it still gets into the catalog and is loaded by GUID.
            if (addressableSettings.FindAssetEntry(guid, true) == null) return false;

            key = guid;
            return true;
        }
    }

}
