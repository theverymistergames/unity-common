using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var hasValueProperty = property.FindPropertyRelative("_hasValue");
            EditorGUI.PropertyField(position, hasValueProperty, label);

            EditorGUI.BeginDisabledGroup(!hasValueProperty.boolValue);

            var valueProperty = property.FindPropertyRelative("_value");
            var valueRect = new Rect(
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
