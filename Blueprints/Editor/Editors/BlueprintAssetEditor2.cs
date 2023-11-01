using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Editors {

    [CustomEditor(typeof(BlueprintAsset2))]
    public sealed class BlueprintAssetEditor2 : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (target is not BlueprintAsset2 blueprint) return;

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.IntField("Nodes count", blueprint.BlueprintMeta.NodeCount);

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            var subgraphBlueprints = blueprint.BlueprintMeta.SubgraphAssets;
            if (subgraphBlueprints.Count > 0) {
                GUILayout.Label("Subgraph blueprints", EditorStyles.boldLabel);

                foreach (var subgraph in subgraphBlueprints) {
                    DrawSubgraphBlueprintsRecursively(subgraph, 0);
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_blackboard"));

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
        }

        private static void DrawSubgraphBlueprintsRecursively(BlueprintAsset2 blueprint, int depth) {
            if (blueprint == null) return;

            EditorGUILayout.ObjectField($"Subgraph (depth {depth})", blueprint, typeof(BlueprintAsset), false);

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            foreach (var subgraph in blueprint.BlueprintMeta.SubgraphAssets) {
                DrawSubgraphBlueprintsRecursively(subgraph, depth + 1);
            }
        }
    }

}
