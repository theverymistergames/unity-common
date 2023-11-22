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

            var subgraphAssetMap = blueprint.BlueprintMeta.SubgraphAssetMap;
            if (subgraphAssetMap.Count > 0) {
                GUILayout.Label("Subgraph blueprints", EditorStyles.boldLabel);

                foreach (var (id, subgraph) in subgraphAssetMap) {
                    DrawSubgraphBlueprintsRecursively(id, subgraph);
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

        private static void DrawSubgraphBlueprintsRecursively(NodeId id, BlueprintAsset2 blueprint) {
            if (blueprint == null) return;

            EditorGUILayout.ObjectField($"Node {id.source}.{id.node}", blueprint, typeof(BlueprintAsset), false);

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            foreach (var (nodeId, subgraph) in blueprint.BlueprintMeta.SubgraphAssetMap) {
                DrawSubgraphBlueprintsRecursively(nodeId, subgraph);
            }
        }
    }

}
