using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var hasValueProperty = property.FindPropertyRelative("_hasValue");
            var valueProperty = property.FindPropertyRelative("_value");

            bool hasLabel = label != null && label != GUIContent.none;
            float offset = hasLabel.AsFloat() * (EditorGUIUtility.labelWidth + 2f);
            float indent = EditorGUI.indentLevel * 15f;
            
            var rect = position;
            rect.x += offset + -indent - EditorGUIUtility.singleLineHeight;
            rect.width = EditorGUIUtility.singleLineHeight;
            rect.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.PropertyField(rect, hasValueProperty, GUIContent.none);

            EditorGUI.BeginDisabledGroup(!hasValueProperty.boolValue);
            EditorGUI.PropertyField(position, valueProperty, label, true);
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_value"));
        }
    }

}
