using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core {

    [CustomEditor(typeof(BlueprintRunner), true)]
    public class BlueprintRunnerDrawer : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (target is not BlueprintRunner runner) return;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            var blueprintAssetProperty = serializedObject.FindProperty("_blueprintAsset");
            EditorGUILayout.PropertyField(blueprintAssetProperty);

            var blueprintAsset = runner.BlueprintAsset;
            if (blueprintAsset != null) {
                if (GUILayout.Button("Edit")) {
                    BlueprintsEditorWindow.OpenAsset(blueprintAsset);
                }

                GUILayout.Space(10);

                var blackboardPropertiesProperty = serializedObject.FindProperty("_blackboardProperties");
                EditorGUILayout.PropertyField(blackboardPropertiesProperty);

                if (GUILayout.Button("Fetch blackboard properties")) {
                    runner.FetchBlackboardGameObjectProperties();
                }
            }

            EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
        }
    }

}
