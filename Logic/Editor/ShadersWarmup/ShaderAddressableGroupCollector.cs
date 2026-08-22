using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace MisterGames.Logic.Editor.ShadersWarmup {

    /// <summary>
    /// Moves all addressable shader entries from all project groups into a single group named Shaders.
    /// <para>
    /// Why. While a shader lies in a group next to content, it goes into that group bundle. The same shader
    /// mentioned in another group goes there as well, and the player ends up with two copies of one shader
    /// with different instance ids: warmup warms up the one the rooms do not render with. One group for all
    /// shaders gives one bundle, the other bundles reference it instead of carrying a copy.
    /// </para>
    /// <para>
    /// What it does. Walks all groups except Shaders and the read only ones, and moves entries whose main asset
    /// is a <see cref="Shader"/> (.shadergraph included). The move keeps the address and the labels of an entry,
    /// only the group, that is the bundle, changes.
    /// </para>
    /// <para>
    /// What it does not do. It does not take apart addressable folders: a shader inside such a folder has no
    /// entry of its own, and pulling it out means changing the folder content, which is a content decision and
    /// not a mechanical sort. Such shaders are only listed in the report. ComputeShader, materials and
    /// ShaderVariantCollection are not touched. Warmup keys are not collected after the move, that is
    /// <see cref="ShaderWarmupAddressableKeys"/>.
    /// </para>
    /// <para>
    /// Next to it lives <see cref="ShaderWarmupAddressableFixer"/>, which solves the opposite task: it creates
    /// entries for the release collection shaders that have no entry at all.
    /// </para>
    /// </summary>
    internal static class ShaderAddressableGroupCollector {

        [MenuItem(ShaderWarmupEditorUtils.MenuRoot + "Report: shaders in foreign groups", priority = 110)]
        private static void Report() {
            Run(dryRun: true);
        }

        [MenuItem(ShaderWarmupEditorUtils.MenuRoot + "Collect all shaders into Shaders group", priority = 111)]
        private static void Collect() {
            Run(dryRun: false);
        }

        private static void Run(bool dryRun) {
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

            if (addressableSettings == null) {
                Debug.LogError($"{nameof(ShaderAddressableGroupCollector)}: Addressables settings are not found. Nothing is done.");
                return;
            }

            var toMove = new List<AddressableAssetEntry>();
            var skippedReadOnly = new List<string>();
            var insideFolders = new List<string>();
            int alreadyInGroup = 0;

            foreach (var group in addressableSettings.groups) {
                if (group == null) continue;

                if (group.Name == ShaderWarmupEditorUtils.ShaderGroupName) {
                    alreadyInGroup += group.entries.Count(IsShaderEntry);
                    continue;
                }

                // Built In Data and other service groups: their content is not defined by the user.
                if (group.ReadOnly) {
                    if (group.entries.Any(IsShaderEntry)) skippedReadOnly.Add(group.Name);
                    continue;
                }

                // A copy: CreateOrMoveEntry modifies the collection being iterated.
                foreach (var entry in group.entries.ToList()) {
                    if (entry == null) continue;

                    if (entry.IsFolder) {
                        CollectShadersInsideFolder(entry, insideFolders);
                        continue;
                    }

                    if (IsShaderEntry(entry)) toMove.Add(entry);
                }
            }

            // Source group names are taken before the move: after it parentGroup of an entry is the target one.
            var sourceGroups = SummarizeSourceGroups(toMove);

            if (!dryRun && toMove.Count > 0) {
                if (!Confirm(toMove.Count, sourceGroups)) return;

                MoveEntries(addressableSettings, toMove);
            }

            LogReport(dryRun, toMove.Count, sourceGroups, alreadyInGroup, skippedReadOnly, insideFolders);
        }

        private static bool IsShaderEntry(AddressableAssetEntry entry) {
            // MainAssetType reads the type by the asset path, so .shadergraph arrives here as a Shader too.
            return entry != null
                   && !entry.IsFolder
                   && !entry.IsSubAsset
                   && typeof(Shader).IsAssignableFrom(entry.MainAssetType);
        }

        /// <summary> Shaders inside an addressable folder: they have no entry, there is nothing to move, only to show. </summary>
        private static void CollectShadersInsideFolder(AddressableAssetEntry folderEntry, List<string> results) {
            string folderPath = folderEntry.AssetPath;

            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath)) return;

            string[] guids = AssetDatabase.FindAssets("t:Shader", new[] { folderPath });

            for (int i = 0; i < guids.Length; i++) {
                results.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
            }
        }

        /// <summary> Source groups as "name - entry count", from bigger to smaller. </summary>
        private static List<string> SummarizeSourceGroups(List<AddressableAssetEntry> entries) {
            return entries.Where(entry => entry.parentGroup != null)
                          .GroupBy(entry => entry.parentGroup.Name)
                          .OrderByDescending(group => group.Count())
                          .Select(group => $"{group.Key} - {group.Count()}")
                          .ToList();
        }

        private static bool Confirm(int entryCount, List<string> sourceGroups) {
            return EditorUtility.DisplayDialog(
                nameof(ShaderAddressableGroupCollector),
                $"Move {entryCount} shader entries into group [{ShaderWarmupEditorUtils.ShaderGroupName}]?\n\n" +
                $"Source groups: {sourceGroups.Count}\n" +
                $"{string.Join("\n", sourceGroups.Take(ShaderWarmupEditorUtils.ReportSampleSize))}" +
                (sourceGroups.Count > ShaderWarmupEditorUtils.ReportSampleSize ? "\n..." : string.Empty) +
                "\n\nAddresses and labels are kept, only the group (that is the bundle) changes.",
                "Move",
                "Cancel");
        }

        private static void MoveEntries(AddressableAssetSettings addressableSettings, List<AddressableAssetEntry> entries) {
            var group = ShaderWarmupEditorUtils.GetOrCreateShaderGroup(addressableSettings);

            try {
                for (int i = 0; i < entries.Count; i++) {
                    var entry = entries[i];

                    if (EditorUtility.DisplayCancelableProgressBar(
                            nameof(ShaderAddressableGroupCollector),
                            $"{entry.address} ({i + 1}/{entries.Count})",
                            (float) i / entries.Count))
                    {
                        Debug.LogWarning($"{nameof(ShaderAddressableGroupCollector)}: cancelled by user at " +
                                         $"{i} of {entries.Count}. Already moved entries stay in " +
                                         $"[{ShaderWarmupEditorUtils.ShaderGroupName}].");
                        break;
                    }

                    // readOnly: false, the entry stays a normal editable one, address and labels move with it.
                    // postEvent: false on every step, otherwise the Addressables window rebuilds on every entry.
                    addressableSettings.CreateOrMoveEntry(entry.guid, group, false, false);
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
            int movedCount,
            List<string> sourceGroups,
            int alreadyInGroup,
            List<string> skippedReadOnly,
            List<string> insideFolders)
        {
            var report = new StringBuilder();

            report.Append(nameof(ShaderAddressableGroupCollector))
                  .Append(dryRun ? " (report, nothing is changed)" : string.Empty)
                  .Append(": group [").Append(ShaderWarmupEditorUtils.ShaderGroupName).Append("] already holds shaders: ")
                  .Append(alreadyInGroup).Append('.');

            report.Append(dryRun ? " Can be moved: " : " Moved: ").Append(movedCount).Append('.');

            if (movedCount > 0) {
                report.Append(" From groups: ").Append(ShaderWarmupEditorUtils.FormatSample(sourceGroups)).Append('.');
            }

            if (skippedReadOnly.Count > 0) {
                report.Append(" Skipped read only groups (their content is not defined by the user): ")
                      .Append(ShaderWarmupEditorUtils.FormatSample(skippedReadOnly)).Append('.');
            }

            if (insideFolders.Count > 0) {
                report.Append(" Inside addressable folders, have no entry of their own and therefore not touched: ")
                      .Append(insideFolders.Count).Append(" (")
                      .Append(ShaderWarmupEditorUtils.FormatSample(insideFolders)).Append(").");
            }

            if (!dryRun && movedCount > 0) {
                report.Append(" Next, run [Collect addressable keys of shaders and vfx assets], ")
                      .Append("otherwise warmup keeps warming shaders up by the old keys.");
            }

            Debug.Log(report.ToString());
        }
    }

}
