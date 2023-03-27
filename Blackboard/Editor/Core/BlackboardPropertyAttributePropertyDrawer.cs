using MisterGames.Blackboards.Core;
using MisterGames.Common.Editor.Drawers;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blackboards.Editor {

    [CustomPropertyDrawer(typeof(BlackboardPropertyAttribute))]
    public sealed class BlackboardPropertyAttributePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var attr = (BlackboardPropertyAttribute) attribute;
            if (property.propertyType != SerializedPropertyType.Integer ||
                property.serializedObject.FindProperty(attr.pathToBlackboard)?.GetValue() is not Blackboard blackboard
            ) {
                PropertyDrawerUtils.DrawPropertyField(
                    position,
                    property,
                    label,
                    SerializedPropertyExtensions.GetPropertyFieldInfo(property),
                    includeChildren: true
                );
                EditorGUI.EndProperty();
                return;
            }

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            GUI.Label(labelRect, label);

            float popupWidth = label == GUIContent.none || string.IsNullOrEmpty(label.text)
                ? position.width - 14f
                : position.width - EditorGUIUtility.labelWidth;

            var popupRect = new Rect(position.x + position.width - popupWidth, position.y, popupWidth, EditorGUIUtility.singleLineHeight);

            int propertiesCount = blackboard.Properties.Count;
            int propertyHash = property.intValue;
            int selectedIndex = 0;

            string[] items = new string[propertiesCount + 1];
            items[0] = "<null>";

            for (int i = 0; i < blackboard.Properties.Count; i++) {
                int hash = blackboard.Properties[i];
                blackboard.TryGetProperty(hash, out var p);

                items[i + 1] = p.name;

                if (hash == propertyHash) selectedIndex = i + 1;
            }

            int newSelectedIndex = EditorGUI.Popup(popupRect, string.Empty, selectedIndex, items);

            if (newSelectedIndex != selectedIndex) {
                property.intValue = newSelectedIndex == 0 ? default : blackboard.Properties[newSelectedIndex - 1];

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var attr = (BlackboardPropertyAttribute) attribute;
            return property.propertyType == SerializedPropertyType.Integer &&
                   property.serializedObject.FindProperty(attr.pathToBlackboard)?.GetValue() is Blackboard
                ? EditorGUIUtility.singleLineHeight
                : PropertyDrawerUtils.GetPropertyHeight(
                    property,
                    label,
                    SerializedPropertyExtensions.GetPropertyFieldInfo(property),
                    includeChildren: true
                );
        }
    }

}
