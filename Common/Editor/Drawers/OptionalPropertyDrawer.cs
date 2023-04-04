using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.LabelField(position, label);

            var hasValueProperty = property.FindPropertyRelative("_hasValue");
            float hasValueSize = EditorGUIUtility.singleLineHeight;

            var hasValueRect = new Rect(
                position.x + EditorGUIUtility.labelWidth + 2f,
                position.y,
                hasValueSize,
                hasValueSize
            );
            EditorGUI.PropertyField(hasValueRect, hasValueProperty, GUIContent.none);

            EditorGUI.BeginDisabledGroup(!hasValueProperty.boolValue);

            var valueRect = new Rect(
                position.x + EditorGUIUtility.labelWidth + hasValueSize + 2f,
                position.y,
                position.width - EditorGUIUtility.labelWidth - hasValueSize - 2f,
                position.height
            );
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("_value"), GUIContent.none, true);

            EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_value"));
        }
    }

}
