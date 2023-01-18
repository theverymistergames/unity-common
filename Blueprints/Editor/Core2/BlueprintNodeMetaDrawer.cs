using MisterGames.Blueprints.Core2;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core2 {

    [CustomEditor(typeof(BlueprintNodeMeta), true)]
    public class BlueprintNodeMetaDrawer : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (serializedObject.targetObject == null) return;
            
            serializedObject.Update();
            
            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;
            
            EditorGUIUtility.labelWidth = 110;
            EditorGUIUtility.fieldWidth = 160;

            var nodeProperty = serializedObject.FindProperty("_node");
            foreach (object child in nodeProperty) {
                var childProperty = (SerializedProperty) child;
                EditorGUILayout.PropertyField(childProperty, true);

                if (childProperty.GetValue() is BlueprintAsset blueprintAsset && GUILayout.Button("Edit")) {
                    BlueprintsEditorWindow.GetWindow().PopulateFromAsset(blueprintAsset);
                }
            }

            serializedObject.ApplyModifiedProperties();
            
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = fieldWidth;
        }
        
    }
}
