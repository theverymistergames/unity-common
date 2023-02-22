using System;
using System.Collections.Generic;
using System.Reflection;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Blackboards.Core.Blackboard;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Editor {

    public static class BlackboardUtils {

        public static VisualElement CreateBlackboardPropertyView(
            SerializedBlackboardProperty property,
            Action<BlackboardProperty, object> onSetPropertyValue,
            Action onPropertyValueChanged
        ) {
            var type = (Type) property.blackboardProperty.type;
            string typeName = TypeNameFormatter.GetTypeName(type);
            string propertyName = property.blackboardProperty.name;
            var nameField = new BlackboardField { text = propertyName, typeText = typeName };

            var valueField = type == null
                ? new VisualElement()
                : typeof(Object).IsAssignableFrom(type) ? CreateObjectField(property, onPropertyValueChanged)
                : type.IsEnum ? CreateEnumField(property, onSetPropertyValue)
                : CreatePropertyField(property, onPropertyValueChanged);

            var container = new VisualElement();
            container.Add(nameField);
            container.Add(new BlackboardRow(nameField, valueField) { expanded = true });
            return container;
        }

        private static VisualElement CreateObjectField(SerializedBlackboardProperty property, Action onPropertyValueChanged) {
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

                onPropertyValueChanged.Invoke();
            });

            return objectField;
        }

        private static VisualElement CreateEnumField(SerializedBlackboardProperty property, Action<BlackboardProperty, object> onSetPropertyValue) {
            var type = (Type) property.blackboardProperty.type;

            if (type.GetCustomAttribute<FlagsAttribute>(false) != null) {
                var currentEnumFlagsValue = Enum.ToObject(type, property.serializedProperty.intValue) as Enum;
                var enumFlagsField = new EnumFlagsField(currentEnumFlagsValue) { label = string.Empty };

                enumFlagsField.RegisterValueChangedCallback(e => {
                    if (Equals(e.newValue, e.previousValue)) return;

                    onSetPropertyValue.Invoke(property.blackboardProperty, Enum.ToObject(type, e.newValue));
                });

                return enumFlagsField;
            }

            var currentEnumValue = Enum.ToObject(type, property.serializedProperty.intValue) as Enum;
            var enumField = new EnumField(currentEnumValue) { label = string.Empty };

            enumField.RegisterValueChangedCallback(e => {
                if (Equals(e.newValue, e.previousValue)) return;

                onSetPropertyValue.Invoke(property.blackboardProperty, Enum.ToObject(type, e.newValue));
            });

            return enumField;
        }

        private static VisualElement CreatePropertyField(SerializedBlackboardProperty property, Action onPropertyValueChanged) {
            var type = (Type) property.blackboardProperty.type;

            object currentValue = property.serializedProperty.GetValue();

            var propertyField = new PropertyField {
                bindingPath = property.serializedProperty.propertyPath,
                label = string.Empty
            };

            propertyField.Bind(property.serializedProperty.serializedObject);
            propertyField.RegisterValueChangeCallback(e => {
                object value = e.changedProperty.GetValue();
                if (type.IsValueType && Equals(value, currentValue)) return;

                currentValue = value;
                onPropertyValueChanged.Invoke();
            });

            return propertyField;
        }

        private const string BOOLS = "_bools";
        private const string LONGS = "_longs";
        private const string DOUBLES = "_doubles";
        private const string STRINGS = "_strings";
        private const string VECTORS2 = "_vectors2";
        private const string VECTORS3 = "_vectors3";
        private const string VECTORS4 = "_vectors4";
        private const string VECTORS2_INT = "_vectors2Int";
        private const string VECTORS3_INT = "_vectors3Int";
        private const string QUATERNIONS = "_quaternions";
        private const string COLORS = "_colors";
        private const string LAYER_MASKS = "_layerMasks";
        private const string CURVES = "_curves";
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
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(LONGS), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(DOUBLES), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(STRINGS), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(VECTORS2), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(VECTORS3), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(VECTORS4), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(VECTORS2_INT), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(VECTORS3_INT), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(QUATERNIONS), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(COLORS), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(LAYER_MASKS), propertiesMap, properties);
            FetchBlackboardDictionary(VALUE, blackboardSerializedProperty.FindPropertyRelative(CURVES), propertiesMap, properties);
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
