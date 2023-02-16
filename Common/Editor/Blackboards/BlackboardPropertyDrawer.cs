using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Blackboards {

    [CustomPropertyDrawer(typeof(Blackboard))]
    public class BlackboardPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(headerRect, label);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, GUIContent.none, toggleOnLabelClick: false);

            if (!property.isExpanded) return;

            EditorGUI.indentLevel++;

            float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var properties = BlackboardUtils.GetSerializedBlackboardProperties(property);
            if (properties.Count == 0) {
                EditorGUI.HelpBox(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "Blackboard has no properties", MessageType.None);
                return;
            }

            var blackboard = (Blackboard) property.GetValue();
            var overridenBlackboard = blackboard.OverridenBlackboard;

            for (int i = 0; i < properties.Count; i++) {
                var propertyData = properties[i];
                var elementProperty = propertyData.serializedProperty;

                if (elementProperty == null) {
                    y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    continue;
                }

                float propertyHeight = EditorGUI.GetPropertyHeight(elementProperty);
                var rect = new Rect(position.x, y, position.width, propertyHeight);

                bool hasOverride =
                    overridenBlackboard != null &&
                    overridenBlackboard.TryGetPropertyValue(propertyData.hash, out object overridenValue) &&
                    blackboard.TryGetPropertyValue(propertyData.hash, out object value) &&
                    (
                        value != null &&
                        overridenValue != null &&
                        value.GetType() == overridenValue.GetType() &&
                        !Equals(value, overridenValue) ||
                        (value == null) != (overridenValue == null)
                    );

                var currentFontStyle = EditorStyles.label.fontStyle;
                if (hasOverride) EditorStyles.label.fontStyle = FontStyle.Bold;

                if (typeof(Object).IsAssignableFrom(propertyData.blackboardProperty.type)) {
                    EditorGUI.ObjectField(rect, elementProperty, propertyData.blackboardProperty.type, new GUIContent(propertyData.blackboardProperty.name));
                }
                else {
                    EditorGUI.PropertyField(rect, elementProperty, new GUIContent(propertyData.blackboardProperty.name));
                }

                if (hasOverride) EditorStyles.label.fontStyle = currentFontStyle;

                y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;

            height += EditorGUIUtility.standardVerticalSpacing;

            var properties = BlackboardUtils.GetSerializedBlackboardProperties(property);

            if (properties.Count == 0) {
                height += EditorGUIUtility.singleLineHeight;
                return height;
            }

            for (int i = 0; i < properties.Count; i++) {
                var elementProperty = properties[i].serializedProperty;

                float propertyHeight = elementProperty == null
                    ? EditorGUIUtility.singleLineHeight
                    : EditorGUI.GetPropertyHeight(elementProperty);

                height += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }

}
