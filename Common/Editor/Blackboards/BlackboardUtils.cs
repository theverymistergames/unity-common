using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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

                    var type = value?.GetType();
                    if (type == null) return;

                    if (type.IsValueType && Equals(value, currentValue)) return;

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

        private const string BOOLS = "_bools";
        private const string FLOATS = "_floats";
        private const string INTS = "_ints";
        private const string STRINGS = "_strings";
        private const string VECTORS2 = "_vectors2";
        private const string VECTORS3 = "_vectors3";
        private const string OBJECTS = "_objects";
        private const string REFERENCES = "_references";

        private const string VALUE = "value";
        private const string VALUE_DATA = "value.data";

        private const string ENTRIES = "_entries";
        private const string KEY = "key";

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

            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(BOOLS), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(FLOATS), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(INTS), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(STRINGS), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(VECTORS2), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(VECTORS3), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(OBJECTS), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE_DATA, blackboardSerializedProperty.FindPropertyRelative(REFERENCES), propertiesMap, properties);

            return properties;
        }

        private static void FetchBlackboardDictionary(
            string valuePropertyPath,
            SerializedProperty dictionary,
            IReadOnlyDictionary<int, (int index, BlackboardProperty property)> propertiesMap,
            IList<SerializedBlackboardProperty> dest
        ) {
            var entries = dictionary.FindPropertyRelative(ENTRIES);

            for (int e = 0; e < entries.arraySize; e++) {
                var entry = entries.GetArrayElementAtIndex(e);
                int hash = entry.FindPropertyRelative(KEY).intValue;

                if (!propertiesMap.TryGetValue(hash, out var p)) continue;

                dest[p.index] = new SerializedBlackboardProperty(p.property, entry.FindPropertyRelative(valuePropertyPath));
            }
        }
    }

}
