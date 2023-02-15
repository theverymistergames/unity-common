using System;
using System.Collections;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(Blackboard))]
    public class BlackboardPropertyDrawer : PropertyDrawer {

        private readonly struct PropertyData {
            public readonly int hash;
            public readonly BlackboardProperty blackboardProperty;
            public readonly SerializedProperty property;

            public PropertyData(int hash, BlackboardProperty blackboardProperty, SerializedProperty property) {
                this.hash = hash;
                this.blackboardProperty = blackboardProperty;
                this.property = property;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(headerRect, label);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, GUIContent.none, toggleOnLabelClick: false);

            if (!property.isExpanded) return;

            EditorGUI.indentLevel++;

            float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var properties = FetchBlackboardProperties(property);
            if (properties.Count == 0) {
                EditorGUI.HelpBox(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "Blackboard has no properties", MessageType.None);
                return;
            }

            var blackboard = (Blackboard) property.GetValue();
            var overridenBlackboard = blackboard.OverridenBlackboard;

            for (int i = 0; i < properties.Count; i++) {
                var propertyData = properties[i];
                var elementProperty = propertyData.property;

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
                        !GetEqualityComparer(value.GetType()).Equals(value, overridenValue) ||
                        (value == null) != (overridenValue == null)
                    );

                var currentFontStyle = EditorStyles.label.fontStyle;
                if (hasOverride) EditorStyles.label.fontStyle = FontStyle.Bold;

                EditorGUI.PropertyField(rect, elementProperty, new GUIContent(propertyData.blackboardProperty.name));

                if (hasOverride) EditorStyles.label.fontStyle = currentFontStyle;

                y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;

            height += EditorGUIUtility.standardVerticalSpacing;

            var properties = FetchBlackboardProperties(property);

            if (properties.Count == 0) {
                height += EditorGUIUtility.singleLineHeight;
                return height;
            }

            for (int i = 0; i < properties.Count; i++) {
                var elementProperty = properties[i].property;

                float propertyHeight = elementProperty == null
                    ? EditorGUIUtility.singleLineHeight
                    : EditorGUI.GetPropertyHeight(elementProperty);

                height += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        private static List<PropertyData> FetchBlackboardProperties(SerializedProperty property) {
            var blackboard = (Blackboard) property.GetValue();
            int propertiesCount = blackboard.Properties.Count;

            var propertiesMap = new Dictionary<int, (int index, BlackboardProperty property)>(propertiesCount);
            var properties = new List<PropertyData>(propertiesMap.Count);

            for (int i = 0; i < propertiesCount; i++) {
                var blackboardProperty = blackboard.Properties[i];

                propertiesMap[blackboardProperty.hash] = (i, blackboardProperty);
                properties.Add(default);
            }

            FetchBlackboardDictionary(property.FindPropertyRelative("_bools"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_floats"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_ints"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_strings"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_vectors2"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_vectors3"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_curves"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_scriptableObjects"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_gameObjects"), propertiesMap, properties);

            return properties;
        }

        private static void FetchBlackboardDictionary(
            SerializedProperty dictionary,
            IReadOnlyDictionary<int, (int index, BlackboardProperty property)> propertiesMap,
            IList<PropertyData> dest
        ) {
            var entries = dictionary.FindPropertyRelative("_entries");

            for (int e = 0; e < entries.arraySize; e++) {
                var entry = entries.GetArrayElementAtIndex(e);
                int hash = entry.FindPropertyRelative("key").intValue;

                if (!propertiesMap.TryGetValue(hash, out var p)) continue;

                dest[p.index] = new PropertyData(hash, p.property, entry.FindPropertyRelative("value"));
            }
        }

        public static IEqualityComparer GetEqualityComparer(Type type) {
            if (type == typeof(bool)) return EqualityComparer<bool>.Default;
            if (type == typeof(float)) return EqualityComparer<float>.Default;
            if (type == typeof(int)) return EqualityComparer<int>.Default;
            if (type == typeof(string)) return EqualityComparer<string>.Default;
            if (type == typeof(Vector2)) return EqualityComparer<Vector2>.Default;
            if (type == typeof(Vector3)) return EqualityComparer<Vector3>.Default;
            if (type == typeof(EasingCurve)) return EqualityComparer<EasingCurve>.Default;
            if (type == typeof(ScriptableObject)) return EqualityComparer<ScriptableObject>.Default;
            if (type == typeof(GameObject)) return EqualityComparer<GameObject>.Default;

            throw new NotSupportedException($"Blackboard field of type {type.Name} is not supported");
        }
    }

}
