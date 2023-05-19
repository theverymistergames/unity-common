using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MisterGames.Common.Dependencies;
using MisterGames.Common.Editor.Attributes.SubclassSelector;
using MisterGames.Common.Types;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Editor.Attributes.ReadOnly {

    [CustomPropertyDrawer(typeof(DependencyResolver))]
    public class DependencyResolverDrawer : PropertyDrawer {

        private static readonly GUIContent LabelResolvedAtRuntime = new GUIContent("(resolved at runtime)");
        private static readonly GUIContent LabelHeaderRuntimeResolve = new GUIContent("Runtime Resolve");
        private static readonly GUIContent LabelInternalResolve = new GUIContent("Resolved internally");
        private static readonly GUIContent LabelSharedResolve = new GUIContent("Resolved in shared dependencies");
        private static readonly GUIContent LabelHeaderSerializedResolve = new GUIContent("Serialized Resolve");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            float y = position.y;

            // Foldout
            EditorGUI.indentLevel--;

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, toggleOnLabelClick: true);

            EditorGUI.indentLevel++;

            if (!property.isExpanded) {
                EditorGUI.EndProperty();
                return;
            }

            // Runtime Resolve: header
            rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.LabelField(rect, LabelHeaderRuntimeResolve, EditorStyles.boldLabel);

            // Runtime Resolve: mode property
            rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var modeProperty = property.FindPropertyRelative("_mode");
            EditorGUI.PropertyField(rect, modeProperty);

            HashSet<Type> allRuntimeOverridenTypes = null;
            var internalRuntimeOverridenTypes = new HashSet<Type>();
            var attrs = fieldInfo.GetCustomAttributes<RuntimeDependencyAttribute>().ToArray();
            for (int i = 0; i < attrs.Length; i++) {
                internalRuntimeOverridenTypes.Add(attrs[i].type);
            }

            // Runtime Resolve: check if mode is shared, if true - draw shared dependencies property
            if (modeProperty.enumValueIndex == 1) {
                rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var sharedDependenciesProperty = property.FindPropertyRelative("_sharedDependencies");
                EditorGUI.PropertyField(rect, sharedDependenciesProperty);

                if (sharedDependenciesProperty.objectReferenceValue is RuntimeDependencyResolver runtimeDependencies) {
                    foreach (var type in internalRuntimeOverridenTypes) {
                        runtimeDependencies.overridenTypes.Add(type);
                    }
                    allRuntimeOverridenTypes = runtimeDependencies.overridenTypes;
                }
            }

            // Runtime Resolve: if has internally resolved dependencies - draw label and list in help box
            if (internalRuntimeOverridenTypes.Count > 0) {
                rect = new Rect(position.x - 1f, y, position.width, EditorGUIUtility.singleLineHeight);
                y += EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(rect, LabelInternalResolve);

                var sb = new StringBuilder();
                foreach (var type in internalRuntimeOverridenTypes) {
                    sb.AppendLine($"- {TypeNameFormatter.GetTypeName(type)}");
                }

                float h = Mathf.Max(EditorGUIUtility.singleLineHeight, internalRuntimeOverridenTypes.Count * EditorGUIUtility.singleLineHeight * 0.8f);
                rect = new Rect(position.x, y, position.width, h);
                y += h + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.HelpBox(rect, sb.ToString(), MessageType.None);
            }

            // Runtime Resolve: if has externally resolved dependencies - draw label and list in help box
            if (modeProperty.enumValueIndex == 1 && allRuntimeOverridenTypes is { Count: > 0 }) {
                StringBuilder sb = null;
                int addExternalRuntimeOverridenTypesCount = 0;

                foreach (var type in allRuntimeOverridenTypes) {
                    if (internalRuntimeOverridenTypes.Contains(type)) continue;

                    sb ??= new StringBuilder();
                    sb.AppendLine($"- {TypeNameFormatter.GetTypeName(type)}");

                    addExternalRuntimeOverridenTypesCount++;
                }

                if (addExternalRuntimeOverridenTypesCount > 0) {
                    rect = new Rect(position.x - 1f, y, position.width, EditorGUIUtility.singleLineHeight);
                    y += EditorGUIUtility.singleLineHeight;

                    EditorGUI.LabelField(rect, LabelSharedResolve);

                    float h = Mathf.Max(EditorGUIUtility.singleLineHeight, addExternalRuntimeOverridenTypesCount * EditorGUIUtility.singleLineHeight * 0.8f);
                    rect = new Rect(position.x, y, position.width, h);
                    y += h + EditorGUIUtility.standardVerticalSpacing;

                    EditorGUI.HelpBox(rect, sb!.ToString(), MessageType.None);
                }
            }

            var properties = GetProperties(property);
            if (properties.Count > 0) {
                // Space between Runtime Resolve and Serialized Resolve sections
                y += EditorGUIUtility.standardVerticalSpacing * 3f;

                // Serialized Resolve: header
                rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.LabelField(rect, LabelHeaderSerializedResolve, EditorStyles.boldLabel);
            }

            // Serialized Resolve: dependencies in categories
            bool hasAtLeastOneCategory = false;
            string category = null;

            for (int i = 0; i < properties.Count; i++) {
                var propertyData = properties[i];

                var serializedProperty = propertyData.property;
                if (serializedProperty == null) {
                    y += EditorGUIUtility.singleLineHeight;
                    continue;
                }

                if (propertyData.category != category || !hasAtLeastOneCategory) {
                    if (hasAtLeastOneCategory) {
                        y += EditorGUIUtility.standardVerticalSpacing * 2f;
                    }
                    else {
                        hasAtLeastOneCategory = true;
                    }

                    category = propertyData.category;

                    rect = new Rect(position.x - 1f, y, position.width, EditorGUIUtility.singleLineHeight);
                    y += EditorGUIUtility.singleLineHeight;

                    EditorGUI.LabelField(rect, category, EditorStyles.miniLabel);
                }

                bool isOverriden = (allRuntimeOverridenTypes ?? internalRuntimeOverridenTypes).Contains(propertyData.type);

                float propertyHeight = GetDependencyPropertyHeight(
                    serializedProperty,
                    propertyData.type,
                    label,
                    isOverriden,
                    includeChildren: true
                );

                rect = new Rect(position.x, y, position.width, propertyHeight);
                y += propertyHeight;

                DrawDependencyProperty(
                    rect,
                    serializedProperty,
                    propertyData.type,
                    new GUIContent(propertyData.name),
                    isOverriden,
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
            bool isOverriden,
            bool includeChildren = false
        ) {
            if (isOverriden) {
                EditorGUI.LabelField(position, label);

                var rect = new Rect(
                    position.x + EditorGUIUtility.labelWidth,
                    position.y,
                    position.width - EditorGUIUtility.labelWidth,
                    EditorGUIUtility.singleLineHeight
                );
                EditorGUI.LabelField(rect, LabelResolvedAtRuntime, EditorStyles.whiteMiniLabel);

                return;
            }

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
            bool isOverriden,
            bool includeChildren = false
        ) {
            if (isOverriden) {
                return EditorGUIUtility.singleLineHeight;
            }

            if (type.IsSubclassOf(typeof(object)) || type.IsInterface) {
                return SubclassSelectorGUI.GetPropertyHeight(property, label, includeChildren);
            }

            return EditorGUI.GetPropertyHeight(property, label, includeChildren);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // Foldout
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;

            // Foldout spacing
            height += EditorGUIUtility.standardVerticalSpacing;

            // Runtime Resolve: header
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Runtime Resolve: mode property
            var modeProperty = property.FindPropertyRelative("_mode");
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            HashSet<Type> allRuntimeOverridenTypes = null;
            var internalRuntimeOverridenTypes = new HashSet<Type>();
            var attrs = fieldInfo.GetCustomAttributes<RuntimeDependencyAttribute>().ToArray();
            for (int i = 0; i < attrs.Length; i++) {
                internalRuntimeOverridenTypes.Add(attrs[i].type);
            }

            // Runtime Resolve: check if mode is shared, if true - draw shared dependencies property
            if (modeProperty.enumValueIndex == 1) {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var runtimeDependenciesProperty = property.FindPropertyRelative("_sharedDependencies");
                if (runtimeDependenciesProperty.objectReferenceValue is RuntimeDependencyResolver runtimeDependencies) {
                    allRuntimeOverridenTypes = runtimeDependencies.overridenTypes;
                }
            }

            // Runtime Resolve: if has internally resolved dependencies - draw label and list in help box
            if (internalRuntimeOverridenTypes.Count > 0) {
                height += EditorGUIUtility.singleLineHeight;
                height += Mathf.Max(EditorGUIUtility.singleLineHeight, internalRuntimeOverridenTypes.Count * EditorGUIUtility.singleLineHeight * 0.8f);
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            // Runtime Resolve: if has externally resolved dependencies - draw label and list in help box
            if (modeProperty.enumValueIndex == 1 && allRuntimeOverridenTypes is { Count: > 0 }) {
                int addExternalRuntimeOverridenTypesCount = 0;

                foreach (var type in allRuntimeOverridenTypes) {
                    if (internalRuntimeOverridenTypes.Contains(type)) continue;
                    addExternalRuntimeOverridenTypesCount++;
                }

                if (addExternalRuntimeOverridenTypesCount > 0) {
                    height += EditorGUIUtility.singleLineHeight;
                    height += Mathf.Max(EditorGUIUtility.singleLineHeight, addExternalRuntimeOverridenTypesCount * EditorGUIUtility.singleLineHeight * 0.8f);
                    height += EditorGUIUtility.standardVerticalSpacing;
                }
            }

            var properties = GetProperties(property);
            if (properties.Count > 0) {
                // Space between Runtime Resolve and Serialized Resolve sections
                height += EditorGUIUtility.standardVerticalSpacing * 3f;

                // Serialized Resolve: header
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            // Serialized Resolve: dependencies in categories
            bool hasAtLeastOneCategory = false;
            string category = null;

            for (int i = 0; i < properties.Count; i++) {
                var propertyData = properties[i];

                var serializedProperty = propertyData.property;
                if (serializedProperty == null) {
                    height += EditorGUIUtility.singleLineHeight;
                    continue;
                }

                if (propertyData.category != category || !hasAtLeastOneCategory) {
                    if (hasAtLeastOneCategory) {
                        height += EditorGUIUtility.standardVerticalSpacing * 2f;
                    }
                    else {
                        hasAtLeastOneCategory = true;
                    }

                    category = propertyData.category;
                    height += EditorGUIUtility.singleLineHeight;
                }

                float propertyHeight = GetDependencyPropertyHeight(
                    serializedProperty,
                    propertyData.type,
                    label,
                    isOverriden: (allRuntimeOverridenTypes ?? internalRuntimeOverridenTypes).Contains(propertyData.type),
                    includeChildren: true
                );

                height += propertyHeight;
            }

            height += EditorGUIUtility.standardVerticalSpacing * 2f;

            return height;
        }

        private static List<SerializedDependencyProperty> GetProperties(SerializedProperty dependencyResolverProperty) {
            var metaList = dependencyResolverProperty.FindPropertyRelative("_dependencyMetaList");
            int count = metaList.arraySize;

            var result = new List<SerializedDependencyProperty>(count);

            for (int i = 0; i < count; i++) {
                var meta = metaList.GetArrayElementAtIndex(i);
                var serializedDependencyProperty = CreateSerializedDependencyProperty(dependencyResolverProperty, meta);

                result.Add(serializedDependencyProperty);
            }

            return result;
        }

        private static SerializedDependencyProperty CreateSerializedDependencyProperty(
            SerializedProperty dependencyResolver,
            SerializedProperty dependencyMeta
        ) {
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
