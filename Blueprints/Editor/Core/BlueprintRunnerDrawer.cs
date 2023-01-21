using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core {

    [CustomEditor(typeof(BlueprintRunner), true)]
    public class BlueprintRunnerDrawer : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (target is not BlueprintRunner runner) return;

            var blueprintAsset = runner.BlueprintAsset;

            var blueprintAssetProperty = serializedObject.FindProperty("_blueprintAsset");
            EditorGUILayout.PropertyField(blueprintAssetProperty);

            if (blueprintAsset != null && GUILayout.Button("Edit")) {
                BlueprintsEditorWindow.GetWindow().PopulateFromAsset(blueprintAsset);
            }

            GUILayout.Space(10);

            var blackboardPropertiesProperty = serializedObject.FindProperty("_blackboardProperties");
            EditorGUILayout.PropertyField(blackboardPropertiesProperty);

            if (GUILayout.Button("Fetch blackboard properties")) {
                runner.FetchBlackboardGameObjectProperties();
            }
        }
    }

}
