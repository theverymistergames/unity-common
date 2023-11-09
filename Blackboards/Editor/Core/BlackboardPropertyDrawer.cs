using MisterGames.Blackboards.Core;
using MisterGames.Common.Editor.SerializedProperties;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blackboards.Editor {

    [CustomPropertyDrawer(typeof(Blackboard2))]
    public sealed class BlackboardPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var headerRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.isMouse && e.button == 1 && headerRect.Contains(e.mousePosition)) {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Reset"), false, () => {
                    ((Blackboard2) property.GetValue()).TryResetPropertyValues();

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                });
                menu.ShowAsContext();
            }

            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, toggleOnLabelClick: true);

            if (!property.isExpanded) {
                EditorGUI.EndProperty();
                return;
            }

            float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var blackboard = (Blackboard2) property.GetValue();
            var properties = blackboard.Properties;

            if (properties.Count == 0) {
                var rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(rect, "(no properties)", MessageType.None);
                EditorGUI.EndProperty();
                return;
            }

            for (int i = 0; i < properties.Count; i++) {
                int hash = properties[i];
                string path = blackboard.GetSerializedPropertyPath(hash);
                blackboard.TryGetProperty(hash, out var blackboardProperty);

                var serializedProperty = property.FindPropertyRelative(path);
                if (serializedProperty == null) {
                    y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    continue;
                }

                float propertyHeight = EditorGUI.GetPropertyHeight(serializedProperty, true);
                var rect = new Rect(position.x, y, position.width, propertyHeight);
                y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(rect, serializedProperty, new GUIContent(blackboardProperty.name), includeChildren: true);
            }

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;

            height += EditorGUIUtility.standardVerticalSpacing;

            var blackboard = (Blackboard2) property.GetValue();
            var properties = blackboard.Properties;

            if (properties.Count == 0) {
                height += EditorGUIUtility.singleLineHeight;
                return height;
            }

            for (int i = 0; i < properties.Count; i++) {
                int hash = properties[i];
                var serializedProperty = property.FindPropertyRelative(blackboard.GetSerializedPropertyPath(hash));

                float propertyHeight = serializedProperty == null
                    ? EditorGUIUtility.singleLineHeight
                    : EditorGUI.GetPropertyHeight(serializedProperty, label, includeChildren: true);

                height += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }

}
