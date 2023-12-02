using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Editors {

    [CustomEditor(typeof(BlueprintAsset))]
    public sealed class BlueprintAssetEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (target is not BlueprintAsset blueprint) return;

            EditorGUILayout.IntField("Nodes count", blueprint.BlueprintMeta.NodeCount);

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            var subgraphAssetMap = blueprint.BlueprintMeta.SubgraphAssetMap;
            if (subgraphAssetMap.Count > 0) {
                GUILayout.Label("Subgraph blueprints", EditorStyles.boldLabel);

                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                EditorGUI.BeginDisabledGroup(true);

                foreach (var (id, subgraph) in subgraphAssetMap) {
                    EditorGUILayout.ObjectField($"Node {id.source}.{id.node}", subgraph, typeof(BlueprintAsset), false);
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                }

                EditorGUI.EndDisabledGroup();
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_blackboard"));

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
        }
    }

}
