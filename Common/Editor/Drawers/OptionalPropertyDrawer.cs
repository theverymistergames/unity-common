using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var hasValueProperty = property.FindPropertyRelative("_hasValue");
            var valueProperty = property.FindPropertyRelative("_value");

            var labelRect = new Rect(
                position.x,
                position.y,
                EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight
            );
            if (valueProperty.hasVisibleChildren) {
                labelRect.x += 12f;
                labelRect.width -= 12f;
            }
            EditorGUI.LabelField(labelRect, label);

            var hasValueRect = new Rect(
                position.x + EditorGUIUtility.labelWidth - 12f,
                position.y,
                12f,
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.PropertyField(hasValueRect, hasValueProperty, GUIContent.none);

            EditorGUI.BeginDisabledGroup(!hasValueProperty.boolValue);

            var valueRect = valueProperty.hasVisibleChildren
                ? position
                : new Rect(
                    position.x + EditorGUIUtility.labelWidth + 7f,
                    position.y,
                    position.width - EditorGUIUtility.labelWidth - 7f,
                    position.height
                  );

            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none, true);

            EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_value"));
        }
    }

}
