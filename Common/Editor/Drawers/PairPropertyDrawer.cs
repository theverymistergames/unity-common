using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(Pair<,>))]
    public class PairPropertyDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(
                position.x,
                position.y,
                EditorGUIUtility.singleLineHeight,
                EditorGUIUtility.labelWidth
            );
            EditorGUI.LabelField(labelRect, label);

            var valueRect = new Rect(
                position.x + EditorGUIUtility.labelWidth,
                position.y,
                position.width - EditorGUIUtility.labelWidth,
                position.height
            );

            float halfWidth = valueRect.width * 0.5f;
            var valueARect = new Rect(valueRect.x, valueRect.y, halfWidth, valueRect.height);
            var valueBRect = new Rect(valueRect.x + halfWidth, valueRect.y, halfWidth, valueRect.height);

            var propertyA = property.FindPropertyRelative("_a");
            var propertyB = property.FindPropertyRelative("_b");

            EditorGUI.PropertyField(valueARect, propertyA, GUIContent.none, includeChildren: true);
            EditorGUI.PropertyField(valueBRect, propertyB, GUIContent.none, includeChildren: true);

            EditorGUI.EndProperty();
        }
 
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var propertyA = property.FindPropertyRelative("_a");
            var propertyB = property.FindPropertyRelative("_b");

            float heightA = EditorGUI.GetPropertyHeight(propertyA, GUIContent.none, includeChildren: true);
            float heightB = EditorGUI.GetPropertyHeight(propertyB, GUIContent.none, includeChildren: true);

            return Mathf.Max(heightA, heightB);
        }
    }

}
