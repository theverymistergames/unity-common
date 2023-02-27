using MisterGames.Blackboards.Core;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blackboards.Editor {

    [CustomPropertyDrawer(typeof(Blackboard))]
    public sealed class BlackboardPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, label);

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.isMouse && e.button == 1 && headerRect.Contains(e.mousePosition)) {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Reset"), false, () => {
                    (property.GetValue() as Blackboard)?.TryResetPropertyValues();

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                });
                menu.ShowAsContext();
            }

            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, GUIContent.none, toggleOnLabelClick: false);

            if (!property.isExpanded) {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var properties = BlackboardUtils.GetSerializedBlackboardProperties(property);
            if (properties.Count == 0) {
                var rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(rect, "Blackboard has no properties", MessageType.None);

                EditorGUI.EndProperty();
                return;
            }

            for (int i = 0; i < properties.Count; i++) {
                var propertyData = properties[i];
                var serializedProperty = propertyData.serializedProperty;

                if (serializedProperty == null) {
                    y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    continue;
                }

                float propertyHeight = EditorGUI.GetPropertyHeight(serializedProperty, true);
                var rect = new Rect(position.x, y, position.width, propertyHeight);
                y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(rect, serializedProperty, new GUIContent(propertyData.blackboardProperty.name), true);
            }

            EditorGUI.indentLevel--;

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();

            EditorGUI.EndProperty();
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
                    : EditorGUI.GetPropertyHeight(elementProperty, true);

                height += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }

}
