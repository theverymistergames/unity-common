using MisterGames.Blueprints.Core2;
using UnityEditor;

namespace MisterGames.Blueprints.Editor.Core2 {

    [CustomEditor(typeof(BlueprintNodeMeta), true)]
    public class BlueprintNodeDrawer : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (serializedObject.targetObject == null) return;
            
            serializedObject.Update();
            
            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;
            
            EditorGUIUtility.labelWidth = 110;
            EditorGUIUtility.fieldWidth = 160;

            var property = serializedObject.FindProperty("_node");
            foreach (object child in property) {
                EditorGUILayout.PropertyField((SerializedProperty) child, true);
            }

            serializedObject.ApplyModifiedProperties();
            
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = fieldWidth;
        }
        
    }
}
