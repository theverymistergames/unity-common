using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Common.Data.Blackboard;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Editor.Blackboards {

    public static class BlackboardUtils {

        public static VisualElement CreateBlackboardPropertyView(SerializedBlackboardProperty property, Action onChanged) {
            string typeName = TypeNameFormatter.GetTypeName(property.blackboardProperty.type);
            string propertyName = property.blackboardProperty.name;
            var nameField = new BlackboardField { text = propertyName, typeText = typeName };

            VisualElement valueField;
            if (typeof(Object).IsAssignableFrom(property.blackboardProperty.type)) {
                var currentValue = property.serializedProperty.objectReferenceValue;

                var objectField = new ObjectField {
                    value = currentValue,
                    bindingPath = property.serializedProperty.propertyPath,
                    allowSceneObjects = false,
                    objectType = property.blackboardProperty.type,
                    label = string.Empty
                };

                objectField.Bind(property.serializedProperty.serializedObject);
                objectField.RegisterValueChangedCallback(e => {
                    if (e.newValue == e.previousValue) return;
                    onChanged.Invoke();
                });

                valueField = objectField;
            }
            else {
                object currentValue = property.serializedProperty.GetValue();

                var propertyField = new PropertyField {
                    bindingPath = property.serializedProperty.propertyPath,
                    label = string.Empty
                };

                propertyField.Bind(property.serializedProperty.serializedObject);
                propertyField.RegisterValueChangeCallback(e => {
                    object value = e.changedProperty.GetValue();
                    if (Equals(value, currentValue)) return;

                    currentValue = value;
                    onChanged.Invoke();
                });

                valueField = propertyField;
            }

            var container = new VisualElement();
            container.Add(nameField);
            container.Add(new BlackboardRow(nameField, valueField) { expanded = true });
            return container;
        }

        public static List<SerializedBlackboardProperty> GetSerializedBlackboardProperties(SerializedProperty blackboardSerializedProperty) {
            var blackboard = (Blackboard) blackboardSerializedProperty.GetValue();
            int propertiesCount = blackboard.Properties.Count;

            var propertiesMap = new Dictionary<int, (int index, BlackboardProperty property)>(propertiesCount);
            var properties = new List<SerializedBlackboardProperty>(propertiesMap.Count);

            for (int i = 0; i < propertiesCount; i++) {
                var blackboardProperty = blackboard.Properties[i];

                propertiesMap[blackboardProperty.hash] = (i, blackboardProperty);
                properties.Add(default);
            }

            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative("_bools"), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative("_floats"), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative("_ints"), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative("_strings"), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative("_vectors2"), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative("_vectors3"), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative("_objects"), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative("_references"), propertiesMap, properties);

            return properties;
        }

        private static void FetchBlackboardDictionary(
            SerializedProperty dictionary,
            IReadOnlyDictionary<int, (int index, BlackboardProperty property)> propertiesMap,
            IList<SerializedBlackboardProperty> dest
        ) {
            var entries = dictionary.FindPropertyRelative("_entries");

            for (int e = 0; e < entries.arraySize; e++) {
                var entry = entries.GetArrayElementAtIndex(e);
                int hash = entry.FindPropertyRelative("key").intValue;

                if (!propertiesMap.TryGetValue(hash, out var p)) continue;

                dest[p.index] = new SerializedBlackboardProperty(p.property, entry.FindPropertyRelative("value"));
            }
        }
    }

}
