using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace MisterGames.Logic.Editor.ShadersWarmup {

    /// <summary>
    /// Creates Addressables entries for the shaders of the warmup release collection that have no entry.
    /// <para>
    /// Why. <see cref="ShaderWarmupAddressableKeys"/> looks a key up through FindAssetEntry(guid, includeImplicit: true),
    /// and includeImplicit means "lies inside an addressable folder", not "pulled in as a dependency". A shader
    /// referenced only by an addressable material or prefab has no entry of its own: there is no key for it,
    /// warmup takes it by a direct reference (the copy that arrived in the player with the bootstrap scene),
    /// while the game renders with the bundle copy. In the player log this shows up as the same variant being
    /// uploaded again with another instance id.
    /// </para>
    /// <para>
    /// What it does. Takes the release collection from <c>ShaderWarmupSettings</c>, picks the shaders that are
    /// main assets under Assets/ and have no entry, and puts them into a single Shaders group. An explicit entry
    /// also removes the duplication: the shader goes into its own bundle and the other bundles reference it
    /// instead of carrying a copy.
    /// </para>
    /// <para>
    /// What it does not do. Sub assets of vfx graphs (shaders inside a .vfx), shaders from Library/PackageCache
    /// and built in engine resources cannot get an address, they are only listed in the report. Keys are not
    /// collected after the fix, that is <see cref="ShaderWarmupAddressableKeys"/>.
    /// </para>
    /// </summary>
    internal static class ShaderWarmupAddressableFixer {

        private const string AssetsPrefix = "Assets/";

        [MenuItem(ShaderWarmupEditorUtils.MenuRoot + "Report: shaders with no addressable entry", priority = 100)]
        private static void Report() {
            Run(dryRun: true);
        }

        [MenuItem(ShaderWarmupEditorUtils.MenuRoot + "Create addressable entries for collection shaders", priority = 101)]
        private static void Fix() {
            Run(dryRun: false);
        }

        private static void Run(bool dryRun) {
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

            if (addressableSettings == null) {
                Debug.LogError($"{nameof(ShaderWarmupAddressableFixer)}: Addressables settings are not found. Nothing is done.");
                return;
            }

            if (!ShaderWarmupEditorUtils.TryFindSettings(out var settings)) return;

            var shaders = settings.CollectReleaseCollectionShaders();

            if (shaders.Count == 0) {
                Debug.LogWarning($"{nameof(ShaderWarmupAddressableFixer)}: the release collection of " +
                                 $"[{AssetDatabase.GetAssetPath(settings)}] holds no shaders. Nothing is done.");
                return;
            }

            var toAdd = new List<Shader>();
            var alreadyHaveEntry = new List<Shader>();
            var subAssets = new List<Shader>();
            var outsideAssets = new List<Shader>();

            foreach (var shader in shaders) {
                if (!AssetDatabase.IsMainAsset(shader)) {
                    // Shaders of vfx graphs are sub assets inside a .vfx, they cannot have an entry of their own.
                    subAssets.Add(shader);
                    continue;
                }

                string assetPath = AssetDatabase.GetAssetPath(shader);

                if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith(AssetsPrefix, StringComparison.Ordinal)) {
                    // Packages (Library/PackageCache) and built in engine resources: there is nothing to address.
                    outsideAssets.Add(shader);
                    continue;
                }

                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(shader, out string guid, out long _)
                    || string.IsNullOrEmpty(guid))
                {
                    outsideAssets.Add(shader);
                    continue;
                }

                // includeImplicit: an entry inside an addressable folder counts too, such a shader is not touched
                // in order not to drag it out of a foreign group.
                if (addressableSettings.FindAssetEntry(guid, true) != null) {
                    alreadyHaveEntry.Add(shader);
                    continue;
                }

                toAdd.Add(shader);
            }

            if (!dryRun && toAdd.Count > 0) {
                AddEntries(addressableSettings, toAdd);
            }

            LogReport(dryRun, shaders.Count, toAdd, alreadyHaveEntry, subAssets, outsideAssets);
        }

        private static void AddEntries(AddressableAssetSettings addressableSettings, List<Shader> shaders) {
            var group = ShaderWarmupEditorUtils.GetOrCreateShaderGroup(addressableSettings);

            try {
                for (int i = 0; i < shaders.Count; i++) {
                    var shader = shaders[i];

                    if (EditorUtility.DisplayCancelableProgressBar(
                            nameof(ShaderWarmupAddressableFixer),
                            $"{shader.name} ({i + 1}/{shaders.Count})",
                            (float) i / shaders.Count))
                    {
                        Debug.LogWarning($"{nameof(ShaderWarmupAddressableFixer)}: cancelled by user at " +
                                         $"{i} of {shaders.Count}. Already created entries stay.");
                        break;
                    }

                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(shader, out string guid, out long _);

                    // postEvent: false on every step, otherwise the Addressables window rebuilds on every entry.
                    addressableSettings.CreateOrMoveEntry(guid, group, false, false);
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }

            addressableSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true, true);
            AssetDatabase.SaveAssets();
        }

        private static void LogReport(
            bool dryRun,
            int totalShaders,
            List<Shader> toAdd,
            List<Shader> alreadyHaveEntry,
            List<Shader> subAssets,
            List<Shader> outsideAssets)
        {
            var report = new StringBuilder();

            report.Append(nameof(ShaderWarmupAddressableFixer))
                  .Append(dryRun ? " (report, nothing is changed)" : string.Empty)
                  .Append(": unique shaders in the release collection ").Append(totalShaders).Append('.');

            report.Append(dryRun ? " Entries can be created for: " : " Entries created: ").Append(toAdd.Count)
                  .Append(" (").Append(ShaderWarmupEditorUtils.FormatSample(toAdd)).Append(").");

            report.Append(" Already have an entry: ").Append(alreadyHaveEntry.Count).Append('.');

            if (subAssets.Count > 0) {
                report.Append(" Sub assets of vfx graphs, for which an entry cannot exist: ")
                      .Append(subAssets.Count).Append(" (")
                      .Append(ShaderWarmupEditorUtils.FormatSample(subAssets)).Append(").");
            }

            if (outsideAssets.Count > 0) {
                report.Append(" Outside Assets/ - packages and built in resources, nothing to address: ")
                      .Append(outsideAssets.Count).Append(" (")
                      .Append(ShaderWarmupEditorUtils.FormatSample(outsideAssets)).Append(").");
            }

            if (!dryRun && toAdd.Count > 0) {
                report.Append(" Next, run [Collect addressable keys of shaders and vfx assets], ")
                      .Append("otherwise warmup keeps taking shaders by a direct reference.");
            }

            Debug.Log(report.ToString());
        }
    }

}
