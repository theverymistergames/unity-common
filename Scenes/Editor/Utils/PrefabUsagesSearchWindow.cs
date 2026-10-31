using System.Collections.Generic;
using System.IO;
using MisterGames.Common.Editor.Search;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenes.Editor.Utils {

    public sealed class PrefabUsagesSearchWindow : AssetUsageSearchWindowBase {

        private sealed class TargetsState : ScriptableObject {
            public List<GameObject> targets = new();
        }

        private const string Title = "Prefab Usages Search";

        protected override string PrefsKeyPrefix => nameof(PrefabUsagesSearchWindow);
        protected override string TargetsLabel => "Prefabs";

        [MenuItem("MisterGames/Tools/Prefab Usages Search")]
        private static void Open() {
            GetWindow<PrefabUsagesSearchWindow>(Title);
        }

        [MenuItem("Assets/Find prefab usages")]
        private static void FindSelectedPrefabUsages() {
            var guids = GetSelectedPrefabGuids();
            if (guids.Count == 0) return;

            GetWindow<PrefabUsagesSearchWindow>(Title).OpenTab(guids);
        }

        [MenuItem("Assets/Find prefab usages", isValidateFunction: true)]
        private static bool IsPrefabSelectedValidation() {
            return GetSelectedPrefabGuids().Count > 0;
        }

        private static List<string> GetSelectedPrefabGuids() {
            var guids = new List<string>();
            var objects = Selection.objects;

            for (int i = 0; i < objects.Length; i++) {
                if (!IsPrefabAsset(objects[i] as GameObject, out string guid) || guids.Contains(guid)) continue;

                guids.Add(guid);
            }

            return guids;
        }

        private static bool IsPrefabAsset(GameObject gameObject, out string guid) {
            guid = null;

            return gameObject != null &&
                   PrefabUtility.IsPartOfPrefabAsset(gameObject) &&
                   AssetDatabase.TryGetGUIDAndLocalFileIdentifier(gameObject.transform.root.gameObject, out guid, out long _);
        }

        protected override ScriptableObject CreateTargetsState(IReadOnlyList<string> targets) {
            var state = CreateInstance<TargetsState>();

            for (int i = 0; i < targets.Count; i++) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(targets[i]));
                if (prefab != null) state.targets.Add(prefab);
            }

            return state;
        }

        protected override void ReadTargets(ScriptableObject state, List<string> targets) {
            var prefabs = ((TargetsState) state).targets;

            for (int i = 0; i < prefabs.Count; i++) {
                if (IsPrefabAsset(prefabs[i], out string guid) && !targets.Contains(guid)) targets.Add(guid);
            }
        }

        protected override string GetTargetLabel(string target) {
            string path = AssetDatabase.GUIDToAssetPath(target);
            return string.IsNullOrEmpty(path) ? target : Path.GetFileNameWithoutExtension(path);
        }

        protected override void ConfigureRequest(AssetFileScanner.Request request) { }

        protected override void CollectResults(
            IReadOnlyList<AssetFileScanner.ScannedFile> files,
            IReadOnlyList<string> targets,
            List<ResultGroup> groups
        ) {
            for (int t = 0; t < targets.Count; t++) {
                string guid = targets[t];
                var group = new ResultGroup { Label = GetTargetLabel(guid) };

                for (int f = 0; f < files.Count; f++) {
                    var file = files[f];

                    for (int i = 0; i < file.instances.Count; i++) {
                        var instance = file.instances[i];
                        if (instance.sourcePrefabGuid != guid) continue;

                        group.Rows.Add(new ResultRow {
                            Kind = file.kind,
                            AssetPath = file.assetPath,
                            ObjectPath = instance.path,
                            Info = instance.nestedInPrefabGuid == null ? "Instance" : $"Nested in {GetTargetLabel(instance.nestedInPrefabGuid)}",
                        });
                    }
                }

                groups.Add(group);
            }
        }
    }

}
