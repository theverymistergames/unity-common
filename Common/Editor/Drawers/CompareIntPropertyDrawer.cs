using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(CompareInt))]
    public class CompareIntPropertyDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var compareModeProperty = property.FindPropertyRelative(nameof(CompareInt.mode));
            var valueProperty = property.FindPropertyRelative(nameof(CompareInt.value));

            var compareModeRect = position;
            compareModeRect.width = EditorGUIUtility.labelWidth + (position.width - EditorGUIUtility.labelWidth) * 0.5f - EditorGUIUtility.standardVerticalSpacing;
            
            var valueRect = position;
            valueRect.x += compareModeRect.width + EditorGUIUtility.standardVerticalSpacing;
            valueRect.width = (position.width - EditorGUIUtility.labelWidth) * 0.5f;
            
            EditorGUI.PropertyField(compareModeRect, compareModeProperty, label, includeChildren: true);
            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none, includeChildren: true);

            EditorGUI.EndProperty();
        }
 
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
