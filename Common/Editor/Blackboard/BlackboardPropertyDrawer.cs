using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(Blackboard))]
    public class BlackboardPropertyDrawer : PropertyDrawer {

        private readonly struct PropertyData {
            public readonly int index;
            public readonly string name;
            public readonly SerializedProperty property;

            public PropertyData(int index, string name, SerializedProperty property) {
                this.index = index;
                this.name = name;
                this.property = property;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(headerRect, label);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, GUIContent.none, toggleOnLabelClick: false);

            if (!property.isExpanded) return;

            var properties = FetchBlackboardProperties(property);
            properties.Sort((x, y) => x.index.CompareTo(y.index));

            EditorGUI.indentLevel++;

            float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            for (int i = 0; i < properties.Count; i++) {
                var propertyData = properties[i];
                var elementProperty = propertyData.property;
                if (elementProperty == null) {
                    y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    continue;
                }

                float propertyHeight = EditorGUI.GetPropertyHeight(elementProperty);
                var rect = new Rect(position.x, y, position.width, propertyHeight);
                EditorGUI.PropertyField(rect, elementProperty, new GUIContent(propertyData.name));

                y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;

            height += EditorGUIUtility.standardVerticalSpacing;
            var properties = FetchBlackboardProperties(property);

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
            var propertiesMap = new Dictionary<int, BlackboardProperty>(((Blackboard) property.GetValue()).PropertiesMap);
            var properties = new List<PropertyData>(propertiesMap.Count);

            FetchBlackboardDictionary(property.FindPropertyRelative("_bools"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_floats"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_ints"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_strings"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_vectors2"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_vectors3"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_scriptableObjects"), propertiesMap, properties);
            FetchBlackboardDictionary(property.FindPropertyRelative("_gameObjects"), propertiesMap, properties);

            return properties;
        }

        private static void FetchBlackboardDictionary(
            SerializedProperty dictionary,
            IReadOnlyDictionary<int, BlackboardProperty> propertiesMap,
            ICollection<PropertyData> dest
        ) {
            var entries = dictionary.FindPropertyRelative("_entries");

            for (int e = 0; e < entries.arraySize; e++) {
                var entry = entries.GetArrayElementAtIndex(e);
                int hash = entry.FindPropertyRelative("key").intValue;

                if (!propertiesMap.TryGetValue(hash, out var blackboardProperty)) continue;

                dest.Add(new PropertyData(blackboardProperty.index, blackboardProperty.name, entry.FindPropertyRelative("value")));
            }
        }
    }

}
