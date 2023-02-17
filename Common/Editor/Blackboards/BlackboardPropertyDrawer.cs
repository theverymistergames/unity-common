﻿using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Blackboards {

    [CustomPropertyDrawer(typeof(Blackboard))]
    public class BlackboardPropertyDrawer : PropertyDrawer {

        private FontStyle _editorLabelFontStyleCache;
        private bool _canCacheEditorLabelFontStyle = true;

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
                int hash = propertyData.blackboardProperty.hash;
                var serializedProperty = propertyData.serializedProperty;

                if (serializedProperty == null) {
                    y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    continue;
                }

                float propertyHeight = EditorGUI.GetPropertyHeight(serializedProperty);
                var rect = new Rect(position.x, y, position.width, propertyHeight);
                y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;

                object overridenValue = null;
                object value = null;

                bool hasOverride =
                    overridenBlackboard != null &&
                    overridenBlackboard.TryGetProperty(hash, out var overridenProperty) &&
                    overridenProperty.type == propertyData.blackboardProperty.type &&
                    overridenBlackboard.TryGetPropertyValue(hash, out overridenValue) &&
                    blackboard.TryGetPropertyValue(hash, out value);

                if (_canCacheEditorLabelFontStyle) {
                    _editorLabelFontStyleCache = EditorStyles.label.fontStyle;
                    _canCacheEditorLabelFontStyle = false;
                }

                if (typeof(Object).IsAssignableFrom(propertyData.blackboardProperty.type)) {
                    if (hasOverride) hasOverride = value as Object != overridenValue as Object;
                    if (hasOverride) EditorStyles.label.fontStyle = FontStyle.Bold;

                    EditorGUI.ObjectField(rect, serializedProperty, propertyData.blackboardProperty.type, new GUIContent(propertyData.blackboardProperty.name));
                }
                else {
                    if (hasOverride) hasOverride = !Equals(value, overridenValue);
                    if (hasOverride) EditorStyles.label.fontStyle = FontStyle.Bold;

                    EditorGUI.PropertyField(rect, serializedProperty, new GUIContent(propertyData.blackboardProperty.name));
                }

                EditorStyles.label.fontStyle = _editorLabelFontStyleCache;
                _canCacheEditorLabelFontStyle = true;
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
