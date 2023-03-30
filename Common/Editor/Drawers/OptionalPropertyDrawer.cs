using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var hasValueProperty = property.FindPropertyRelative("_hasValue");
            float hasValueSize = EditorGUIUtility.singleLineHeight;

            EditorGUI.BeginDisabledGroup(!hasValueProperty.boolValue);

            var valueRect = new Rect(position.x, position.y, position.width - hasValueSize, position.height);
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("_value"), label, true);

            EditorGUI.EndDisabledGroup();

            var hasValueRect = new Rect(position.x + position.width - hasValueSize, position.y, hasValueSize, hasValueSize);
            EditorGUI.PropertyField(hasValueRect, hasValueProperty, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_value"));
        }
    }

}
