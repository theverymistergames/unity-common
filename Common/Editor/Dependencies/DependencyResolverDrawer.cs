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

        private readonly HashSet<Type> _internalRuntimeOverridenTypesCache = new HashSet<Type>();

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
            _internalRuntimeOverridenTypesCache.Clear();

            var attrs = fieldInfo.GetCustomAttributes<RuntimeDependencyAttribute>().ToArray();
            for (int i = 0; i < attrs.Length; i++) {
                _internalRuntimeOverridenTypesCache.Add(attrs[i].type);
            }

            // Runtime Resolve: check if mode is shared (enum == 1), if true - draw shared dependencies property
            if (modeProperty.enumValueIndex == 1) {
                rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var sharedDependenciesProperty = property.FindPropertyRelative("_sharedDependencies");
                EditorGUI.PropertyField(rect, sharedDependenciesProperty);

                if (sharedDependenciesProperty.objectReferenceValue is RuntimeDependencyResolver runtimeDependencies) {
                    foreach (var type in _internalRuntimeOverridenTypesCache) {
                        runtimeDependencies.overridenTypes.Add(type);
                    }
                    allRuntimeOverridenTypes = runtimeDependencies.overridenTypes;
                }
            }

            // Runtime Resolve: if has internally resolved dependencies - draw label and list in help box
            if (_internalRuntimeOverridenTypesCache.Count > 0) {
                rect = new Rect(position.x - 1f, y, position.width, EditorGUIUtility.singleLineHeight);
                y += EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(rect, LabelInternalResolve);

                var sb = new StringBuilder();
                foreach (var type in _internalRuntimeOverridenTypesCache) {
                    sb.AppendLine($"- {TypeNameFormatter.GetTypeName(type)}");
                }

                float h = Mathf.Max(EditorGUIUtility.singleLineHeight, _internalRuntimeOverridenTypesCache.Count * EditorGUIUtility.singleLineHeight * 0.8f);
                rect = new Rect(position.x, y, position.width, h);
                y += h + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.HelpBox(rect, sb.ToString(), MessageType.None);
            }

            // Runtime Resolve: if has externally resolved dependencies - draw label and list in help box
            if (modeProperty.enumValueIndex == 1 && allRuntimeOverridenTypes is { Count: > 0 }) {
                StringBuilder sb = null;
                int addExternalRuntimeOverridenTypesCount = 0;

                foreach (var type in allRuntimeOverridenTypes) {
                    if (_internalRuntimeOverridenTypesCache.Contains(type)) continue;

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

            // Buckets
            var bucketsProperty = property.FindPropertyRelative("_buckets");
            int bucketsCount = bucketsProperty.arraySize;

            if (bucketsCount > 0) {
                // Space between Runtime Resolve and Serialized Resolve sections
                y += EditorGUIUtility.standardVerticalSpacing * 6f;

                // Serialized Resolve: header
                rect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.LabelField(rect, LabelHeaderSerializedResolve, EditorStyles.boldLabel);
            }

            var unityObjectsProperty = property.FindPropertyRelative("_unityObjects");
            var objectsProperty = property.FindPropertyRelative("_objects");

            var depMetasProperty = property.FindPropertyRelative("_dependencyMetas");
            var depPointersProperty = property.FindPropertyRelative("_dependencyPointers");

            for (int i = 0; i < bucketsCount; i++) {
                var bucketProperty = bucketsProperty.GetArrayElementAtIndex(i);

                int count = bucketProperty.FindPropertyRelative("count").intValue;
                int offset = bucketProperty.FindPropertyRelative("offset").intValue;
                string name = bucketProperty.FindPropertyRelative("name").stringValue;

                // Bucket header
                rect = new Rect(position.x - 1f, y, position.width, EditorGUIUtility.singleLineHeight);
                y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, name, EditorStyles.miniLabel);

                // Bucket deps
                for (int d = offset; d < count; d++) {
                    var depMetaProperty = depMetasProperty.GetArrayElementAtIndex(d);
                    var depPointerProperty = depPointersProperty.GetArrayElementAtIndex(d);

                    var type = SerializedType.DeserializeType(depMetaProperty.FindPropertyRelative("type._type").stringValue);

                    bool isOverriden = (allRuntimeOverridenTypes ?? _internalRuntimeOverridenTypesCache).Contains(type);
                    var depLabel = new GUIContent(TypeNameFormatter.GetTypeName(type));

                    int list = depPointerProperty.FindPropertyRelative("list").intValue;
                    int index = depPointerProperty.FindPropertyRelative("index").intValue;

                    var serializedProperty = list switch {
                        0 => unityObjectsProperty.GetArrayElementAtIndex(index),
                        1 => objectsProperty.GetArrayElementAtIndex(index),
                        _ => default,
                    };

                    float propertyHeight = GetDependencyPropertyHeight(
                        serializedProperty,
                        type,
                        depLabel,
                        isOverriden,
                        includeChildren: true
                    );

                    rect = new Rect(position.x, y, position.width, propertyHeight);
                    y += propertyHeight;

                    DrawDependencyProperty(
                        rect,
                        serializedProperty,
                        type,
                        depLabel,
                        isOverriden,
                        includeChildren: true
                    );
                }

                // Bucket bottom space
                y += EditorGUIUtility.standardVerticalSpacing * 2f;
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
                EditorGUI.LabelField(rect, LabelResolvedAtRuntime, EditorStyles.miniLabel);

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
            _internalRuntimeOverridenTypesCache.Clear();

            var attrs = fieldInfo.GetCustomAttributes<RuntimeDependencyAttribute>().ToArray();
            for (int i = 0; i < attrs.Length; i++) {
                _internalRuntimeOverridenTypesCache.Add(attrs[i].type);
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
            if (_internalRuntimeOverridenTypesCache.Count > 0) {
                height += EditorGUIUtility.singleLineHeight;
                height += Mathf.Max(EditorGUIUtility.singleLineHeight, _internalRuntimeOverridenTypesCache.Count * EditorGUIUtility.singleLineHeight * 0.8f);
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            // Runtime Resolve: if has externally resolved dependencies - draw label and list in help box
            if (modeProperty.enumValueIndex == 1 && allRuntimeOverridenTypes is { Count: > 0 }) {
                int addExternalRuntimeOverridenTypesCount = 0;

                foreach (var type in allRuntimeOverridenTypes) {
                    if (_internalRuntimeOverridenTypesCache.Contains(type)) continue;
                    addExternalRuntimeOverridenTypesCount++;
                }

                if (addExternalRuntimeOverridenTypesCount > 0) {
                    height += EditorGUIUtility.singleLineHeight;
                    height += Mathf.Max(EditorGUIUtility.singleLineHeight, addExternalRuntimeOverridenTypesCount * EditorGUIUtility.singleLineHeight * 0.8f);
                    height += EditorGUIUtility.standardVerticalSpacing;
                }
            }

            // Buckets
            var bucketsProperty = property.FindPropertyRelative("_buckets");
            int bucketsCount = bucketsProperty.arraySize;

            if (bucketsCount > 0) {
                // Space between Runtime Resolve and Serialized Resolve sections
                height += EditorGUIUtility.standardVerticalSpacing * 6f;

                // Serialized Resolve: header
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            var unityObjectsProperty = property.FindPropertyRelative("_unityObjects");
            var objectsProperty = property.FindPropertyRelative("_objects");

            var depMetasProperty = property.FindPropertyRelative("_dependencyMetas");
            var depPointersProperty = property.FindPropertyRelative("_dependencyPointers");

            for (int i = 0; i < bucketsCount; i++) {
                var bucketProperty = bucketsProperty.GetArrayElementAtIndex(i);

                int count = bucketProperty.FindPropertyRelative("count").intValue;
                int offset = bucketProperty.FindPropertyRelative("offset").intValue;

                // Bucket header
                height += EditorGUIUtility.singleLineHeight;

                // Bucket deps
                for (int d = offset; d < count; d++) {
                    var depMetaProperty = depMetasProperty.GetArrayElementAtIndex(d);
                    var depPointerProperty = depPointersProperty.GetArrayElementAtIndex(d);

                    var type = SerializedType.DeserializeType(depMetaProperty.FindPropertyRelative("type._type").stringValue);

                    bool isOverriden = (allRuntimeOverridenTypes ?? _internalRuntimeOverridenTypesCache).Contains(type);
                    var depLabel = new GUIContent(TypeNameFormatter.GetTypeName(type));

                    int list = depPointerProperty.FindPropertyRelative("list").intValue;
                    int index = depPointerProperty.FindPropertyRelative("index").intValue;

                    var serializedProperty = list switch {
                        0 => unityObjectsProperty.GetArrayElementAtIndex(index),
                        1 => objectsProperty.GetArrayElementAtIndex(index),
                        _ => default,
                    };

                    float propertyHeight = GetDependencyPropertyHeight(
                        serializedProperty,
                        type,
                        depLabel,
                        isOverriden,
                        includeChildren: true
                    );

                    height += propertyHeight;
                }

                // Bucket bottom space
                height += EditorGUIUtility.standardVerticalSpacing * 2f;
            }

            return height;
        }
    }

}
