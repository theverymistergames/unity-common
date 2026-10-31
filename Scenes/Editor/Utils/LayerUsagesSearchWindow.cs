using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Common.Editor.Search;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenes.Editor.Utils {

    public sealed class LayerUsagesSearchWindow : AssetUsageSearchWindowBase {

        private sealed class TargetsState : ScriptableObject {
            [Layer] public List<int> targets = new();
        }

        protected override string PrefsKeyPrefix => nameof(LayerUsagesSearchWindow);
        protected override string TargetsLabel => "Layers";

        [MenuItem("MisterGames/Tools/Find layer usages")]
        private static void Open() {
            GetWindow<LayerUsagesSearchWindow>("Layer Usages Search");
        }

        protected override ScriptableObject CreateTargetsState(IReadOnlyList<string> targets) {
            var state = CreateInstance<TargetsState>();

            for (int i = 0; i < targets.Count; i++) {
                if (int.TryParse(targets[i], out int layer)) state.targets.Add(layer);
            }

            return state;
        }

        protected override void ReadTargets(ScriptableObject state, List<string> targets) {
            var layers = ((TargetsState) state).targets;

            for (int i = 0; i < layers.Count; i++) {
                string layer = layers[i].ToString();
                if (!targets.Contains(layer)) targets.Add(layer);
            }
        }

        protected override string GetTargetLabel(string target) {
            if (!int.TryParse(target, out int layer)) return target;

            string name = LayerMask.LayerToName(layer);
            return string.IsNullOrEmpty(name) ? $"Layer {layer}" : name;
        }

        protected override void ConfigureRequest(AssetFileScanner.Request request) { }

        protected override void CollectResults(
            IReadOnlyList<AssetFileScanner.ScannedFile> files,
            IReadOnlyList<string> targets,
            List<ResultGroup> groups
        ) {
            for (int t = 0; t < targets.Count; t++) {
                if (!int.TryParse(targets[t], out int layer)) continue;

                var group = new ResultGroup { Label = GetTargetLabel(targets[t]) };

                for (int f = 0; f < files.Count; f++) {
                    var file = files[f];

                    for (int i = 0; i < file.objects.Count; i++) {
                        var obj = file.objects[i];
                        if (!obj.isGameObject || obj.layer != layer) continue;

                        group.Rows.Add(new ResultRow {
                            Kind = file.kind,
                            AssetPath = file.assetPath,
                            ObjectPath = obj.path,
                            Info = file.kind,
                        });
                    }
                }

                groups.Add(group);
            }
        }
    }

}
