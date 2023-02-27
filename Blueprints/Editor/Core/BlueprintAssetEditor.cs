using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core {

    [CustomEditor(typeof(BlueprintAsset))]
    public sealed class BlueprintAssetEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (target is not BlueprintAsset blueprint) return;

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.IntField("Nodes count", blueprint.BlueprintMeta.NodesMap.Count);

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            var subgraphBlueprints = blueprint.BlueprintMeta.SubgraphReferencesMap.Values;
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

        private static void DrawSubgraphBlueprintsRecursively(BlueprintAsset blueprint, int depth) {
            if (blueprint == null) return;

            EditorGUILayout.ObjectField($"Subgraph (depth {depth})", blueprint, typeof(BlueprintAsset), false);

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            foreach (var subgraph in blueprint.BlueprintMeta.SubgraphReferencesMap.Values) {
                DrawSubgraphBlueprintsRecursively(subgraph, depth + 1);
            }
        }
    }

}
