using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(CompareInt))]
    public class CompareIntPropertyDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(
                position.x,
                position.y,
                EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.LabelField(labelRect, label);

            var valueRect = new Rect(
                position.x + EditorGUIUtility.labelWidth,
                position.y,
                position.width - EditorGUIUtility.labelWidth,
                position.height
            );

            float halfWidth = valueRect.width * 0.5f;
            var valueARect = new Rect(valueRect.x, valueRect.y, halfWidth - EditorGUIUtility.standardVerticalSpacing, valueRect.height);
            var valueBRect = new Rect(valueRect.x + halfWidth, valueRect.y, halfWidth, valueRect.height);

            var compareModeProperty = property.FindPropertyRelative("_compareMode");
            var compareMode = (CompareMode) compareModeProperty.enumValueIndex;

            var valueProperty = compareMode switch {
                CompareMode.Equal or CompareMode.NotEqual => property.FindPropertyRelative("_values"),
                _ => property.FindPropertyRelative("_value")
            };

            EditorGUI.PropertyField(valueARect, compareModeProperty, GUIContent.none, includeChildren: true);
            EditorGUI.PropertyField(valueBRect, valueProperty, GUIContent.none, includeChildren: true);

            EditorGUI.EndProperty();
        }
 
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var compareModeProperty = property.FindPropertyRelative("_compareMode");
            var compareMode = (CompareMode) compareModeProperty.enumValueIndex;

            var valueProperty = compareMode switch {
                CompareMode.Equal or CompareMode.NotEqual => property.FindPropertyRelative("_values"),
                _ => property.FindPropertyRelative("_value")
            };

            float heightA = EditorGUI.GetPropertyHeight(compareModeProperty, GUIContent.none, includeChildren: true);
            float heightB = EditorGUI.GetPropertyHeight(valueProperty, GUIContent.none, includeChildren: true);

            return Mathf.Max(heightA, heightB);
        }
    }

}
