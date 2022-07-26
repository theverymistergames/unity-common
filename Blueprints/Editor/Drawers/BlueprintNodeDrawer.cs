using MisterGames.Blueprints.Core;
using UnityEditor;

namespace MisterGames.Blueprints.Editor.Drawers {

    [CustomEditor(typeof(BlueprintNode), true)]
    public class BlueprintNodeDrawer : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (serializedObject.targetObject == null) return;
            
            serializedObject.Update();
            
            float labelWidth = EditorGUIUtility.labelWidth;
            float fieldWidth = EditorGUIUtility.fieldWidth;
            
            EditorGUIUtility.labelWidth = 110;
            EditorGUIUtility.fieldWidth = 160;
            
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
            
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = fieldWidth;
        }
        
    }
}