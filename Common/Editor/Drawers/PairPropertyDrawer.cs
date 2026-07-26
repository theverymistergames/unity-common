using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(Pair<,>))]
    public class PairPropertyDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var propertyA = property.FindPropertyRelative("_a");
            var propertyB = property.FindPropertyRelative("_b");

            var rectA = position;
            rectA.width = EditorGUIUtility.labelWidth + (position.width - EditorGUIUtility.labelWidth) * 0.5f - EditorGUIUtility.standardVerticalSpacing;
            
            var rectB = position;
            rectB.x += rectA.width + EditorGUIUtility.standardVerticalSpacing;
            rectB.width = (position.width - EditorGUIUtility.labelWidth) * 0.5f;
            
            EditorGUI.PropertyField(rectA, propertyA, label, includeChildren: true);
            EditorGUI.PropertyField(rectB, propertyB, GUIContent.none, includeChildren: true);

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
