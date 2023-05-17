using System;
using System.Collections.Generic;
using MisterGames.Common.Dependencies;
using MisterGames.Common.Editor.Attributes.SubclassSelector;
using MisterGames.Common.Types;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Editor.Attributes.ReadOnly {

    [CustomPropertyDrawer(typeof(DependencyResolver))]
    public class DependencyResolverDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.indentLevel--;

            var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, toggleOnLabelClick: true);

            EditorGUI.indentLevel++;

            if (!property.isExpanded) {
                EditorGUI.EndProperty();
                return;
            }

            float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var properties = GetProperties(property);
            if (properties.Count == 0) {
                var rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.HelpBox(rect, "No dependencies", MessageType.None);
                EditorGUI.EndProperty();
                return;
            }

            bool hasAtLeastOneCategory = false;
            string category = null;

            for (int i = 0; i < properties.Count; i++) {
                var propertyData = properties[i];
                var serializedProperty = propertyData.property;

                if (serializedProperty == null) {
                    y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    continue;
                }

                if (propertyData.category != category || !hasAtLeastOneCategory) {
                    if (hasAtLeastOneCategory) {
                        y += EditorGUIUtility.standardVerticalSpacing * 3;
                    }
                    else {
                        hasAtLeastOneCategory = true;
                    }

                    category = propertyData.category;

                    var categoryRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                    y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    EditorGUI.LabelField(categoryRect, category);
                }

                float propertyHeight = GetDependencyPropertyHeight(
                    serializedProperty,
                    propertyData.type,
                    label,
                    includeChildren: true
                );

                var rect = new Rect(position.x, y, position.width, propertyHeight);
                y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;

                DrawDependencyProperty(
                    rect,
                    serializedProperty,
                    propertyData.type,
                    new GUIContent(propertyData.name),
                    includeChildren: true
                );
            }

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();

            EditorGUI.EndProperty();
        }

        private static void DrawDependencyProperty(
            Rect position,
            SerializedProperty property,
            Type type,
            GUIContent label,
            bool includeChildren = false
        ) {
            if (typeof(Object).IsAssignableFrom(type)) {
                EditorGUI.ObjectField(position, property, type, label);
                return;
            }

            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                SubclassSelectorGUI.PropertyField(position, property, type, label, includeChildren);
            }
        }

        private static float GetDependencyPropertyHeight(
            SerializedProperty property,
            Type type,
            GUIContent label,
            bool includeChildren = false
        ) {
            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                return SubclassSelectorGUI.GetPropertyHeight(property, label, includeChildren);
            }

            return EditorGUI.GetPropertyHeight(property, label, includeChildren);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;

            height += EditorGUIUtility.standardVerticalSpacing;

            var properties = GetProperties(property);

            if (properties.Count == 0) {
                height += EditorGUIUtility.singleLineHeight;
                return height;
            }

            bool hasAtLeastOneCategory = false;
            string category = null;

            for (int i = 0; i < properties.Count; i++) {
                var propertyData = properties[i];
                var serializedProperty = propertyData.property;

                if (serializedProperty == null) {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    continue;
                }

                if (propertyData.category != category || !hasAtLeastOneCategory) {
                    if (hasAtLeastOneCategory) {
                        height += EditorGUIUtility.standardVerticalSpacing * 2;
                    }
                    else {
                        hasAtLeastOneCategory = true;
                    }

                    category = propertyData.category;
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                float propertyHeight = GetDependencyPropertyHeight(
                    serializedProperty,
                    propertyData.type,
                    label,
                    includeChildren: true
                );

                height += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        private static List<SerializedDependencyProperty> GetProperties(SerializedProperty dependencyResolverProperty) {
            var metaList = dependencyResolverProperty.FindPropertyRelative("_dependencyMetaList");
            int count = metaList.arraySize;

            var result = new List<SerializedDependencyProperty>(count);

            for (int i = 0; i < count; i++) {
                var meta = metaList.GetArrayElementAtIndex(i);
                var serializedDependencyProperty = Create(dependencyResolverProperty, meta);

                result.Add(serializedDependencyProperty);
            }

            return result;
        }

        private static SerializedDependencyProperty Create(SerializedProperty dependencyResolver, SerializedProperty dependencyMeta) {
            string name = dependencyMeta.FindPropertyRelative("name").stringValue;
            string category = dependencyMeta.FindPropertyRelative("category").stringValue;
            var type = SerializedType.DeserializeType(dependencyMeta.FindPropertyRelative("type._type").stringValue);

            int listIndex = dependencyMeta.FindPropertyRelative("listIndex").intValue;
            int elementIndex = dependencyMeta.FindPropertyRelative("elementIndex").intValue;

            var property = listIndex switch {
                0 => dependencyResolver.FindPropertyRelative("_unityObjects").GetArrayElementAtIndex(elementIndex),
                1 => dependencyResolver.FindPropertyRelative("_objects").GetArrayElementAtIndex(elementIndex),
                _ => default,
            };

            return new SerializedDependencyProperty(property, name, category, type);
        }
    }

}
