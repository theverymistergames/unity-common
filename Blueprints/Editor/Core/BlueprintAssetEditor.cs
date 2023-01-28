using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core {

    [CustomEditor(typeof(BlueprintAsset))]
    public sealed class BlueprintAssetEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (target is not BlueprintAsset blueprint) return;

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.IntField("Nodes count", blueprint.BlueprintMeta.NodesMap.Count);

            var subgraphBlueprints = blueprint.BlueprintMeta.SubgraphReferencesMap.Values;
            if (subgraphBlueprints.Count > 0) {
                GUILayout.Space(10);

                GUILayout.Label("Subgraph blueprints", EditorStyles.boldLabel);

                foreach (var subgraph in subgraphBlueprints) {
                    DrawSubgraphBlueprintsRecursively(subgraph);
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void DrawSubgraphBlueprintsRecursively(BlueprintAsset blueprint) {
            if (blueprint == null) return;

            EditorGUILayout.ObjectField($"Subgraph", blueprint, typeof(BlueprintAsset), false);

            foreach (var subgraph in blueprint.BlueprintMeta.SubgraphReferencesMap.Values) {
                DrawSubgraphBlueprintsRecursively(subgraph);
            }
        }
    }

}
