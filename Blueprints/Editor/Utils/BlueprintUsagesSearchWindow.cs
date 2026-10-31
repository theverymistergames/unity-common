using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Common.Editor.Search;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenes.Editor.Utils {

    public sealed class BlueprintUsagesSearchWindow : AssetUsageSearchWindowBase {

        private sealed class TargetsState : ScriptableObject {
            public List<BlueprintAsset> targets = new();
        }

        private const string Title = "Blueprint Usages Search";
        private const string BlueprintAssetFieldName = "_blueprintAsset";

        protected override string PrefsKeyPrefix => nameof(BlueprintUsagesSearchWindow);
        protected override string TargetsLabel => "Blueprints";

        [MenuItem("MisterGames/Tools/Blueprint Usages Search")]
        private static void Open() {
            GetWindow<BlueprintUsagesSearchWindow>(Title);
        }

        [MenuItem("Assets/Find blueprint usages")]
        private static void FindSelectedBlueprintUsages() {
            var guids = GetSelectedBlueprintGuids();
            if (guids.Count == 0) return;

            GetWindow<BlueprintUsagesSearchWindow>(Title).OpenTab(guids);
        }

        [MenuItem("Assets/Find blueprint usages", isValidateFunction: true)]
        private static bool IsBlueprintSelectedValidation() {
            return GetSelectedBlueprintGuids().Count > 0;
        }

        private static List<string> GetSelectedBlueprintGuids() {
            var guids = new List<string>();
            var objects = Selection.objects;

            for (int i = 0; i < objects.Length; i++) {
                if (!TryGetBlueprintGuid(objects[i] as BlueprintAsset, out string guid) || guids.Contains(guid)) continue;

                guids.Add(guid);
            }

            return guids;
        }

        private static bool TryGetBlueprintGuid(BlueprintAsset blueprintAsset, out string guid) {
            guid = null;
            return blueprintAsset != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(blueprintAsset, out guid, out long _);
        }

        protected override ScriptableObject CreateTargetsState(IReadOnlyList<string> targets) {
            var state = CreateInstance<TargetsState>();

            for (int i = 0; i < targets.Count; i++) {
                var blueprintAsset = AssetDatabase.LoadAssetAtPath<BlueprintAsset>(AssetDatabase.GUIDToAssetPath(targets[i]));
                if (blueprintAsset != null) state.targets.Add(blueprintAsset);
            }

            return state;
        }

        protected override void ReadTargets(ScriptableObject state, List<string> targets) {
            var blueprintAssets = ((TargetsState) state).targets;

            for (int i = 0; i < blueprintAssets.Count; i++) {
                if (TryGetBlueprintGuid(blueprintAssets[i], out string guid) && !targets.Contains(guid)) targets.Add(guid);
            }
        }

        protected override string GetTargetLabel(string target) {
            string path = AssetDatabase.GUIDToAssetPath(target);
            return string.IsNullOrEmpty(path) ? target : System.IO.Path.GetFileNameWithoutExtension(path);
        }

        protected override void ConfigureRequest(AssetFileScanner.Request request) {
            request.fieldNames.Add(BlueprintAssetFieldName);
        }

        protected override void CollectResults(
            IReadOnlyList<AssetFileScanner.ScannedFile> files,
            IReadOnlyList<string> targets,
            List<ResultGroup> groups
        ) {
            var groupsByGuid = new Dictionary<string, ResultGroup>();

            for (int t = 0; t < targets.Count; t++) {
                var group = new ResultGroup { Label = GetTargetLabel(targets[t]) };
                groupsByGuid[targets[t]] = group;
                groups.Add(group);
            }

            for (int f = 0; f < files.Count; f++) {
                var file = files[f];

                for (int o = 0; o < file.objects.Count; o++) {
                    var obj = file.objects[o];
                    if (obj.isGameObject || obj.fields == null || !IsBlueprintRunner(obj.scriptGuid)) continue;

                    for (int i = 0; i < obj.fields.Count; i++) {
                        var field = obj.fields[i];
                        if (field.path != BlueprintAssetFieldName ||
                            field.refGuid == null ||
                            !groupsByGuid.TryGetValue(field.refGuid, out var group))
                        {
                            continue;
                        }

                        group.Rows.Add(new ResultRow {
                            Kind = file.kind,
                            AssetPath = file.assetPath,
                            ObjectPath = obj.path,
                            Info = AssetFileScanner.GetScriptType(obj.scriptGuid).Name,
                        });
                    }
                }
            }
        }

        private static bool IsBlueprintRunner(string scriptGuid) {
            var type = AssetFileScanner.GetScriptType(scriptGuid);
            return type != null && typeof(BlueprintRunner).IsAssignableFrom(type);
        }
    }

}
