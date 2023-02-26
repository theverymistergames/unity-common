using System;
using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Blackboards.Core.Blackboard;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Editor {

    public static class BlackboardUtils {

        public static bool TryGetBlackboardProperty(SerializedProperty p, out BlackboardProperty blackboardProperty) {
            string path = p.propertyPath;
            int lastDot = path.LastIndexOf('.');
            path = path.Remove(lastDot, path.Length - lastDot);

            var hashProperty = p.serializedObject.FindProperty($"{path}.key");
            if (hashProperty == null) {
                blackboardProperty = default;
                return false;
            }

            int hash = hashProperty.intValue;

            for (int i = 0; i < 4; i++) {
                lastDot = path.LastIndexOf('.');
                path = path.Remove(lastDot, path.Length - lastDot);
            }

            if (p.serializedObject.FindProperty(path)?.GetValue() is not Blackboard blackboard ||
                !blackboard.TryGetProperty(hash, out blackboardProperty)
            ) {
                blackboardProperty = default;
                return false;
            }

            return true;
        }

        public static void NullPropertyField(Rect position, GUIContent label) {
            var labelRect = new Rect(
                position.x,
                position.y,
                EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.LabelField(labelRect, label);

            var valueRect = new Rect(
                position.x + EditorGUIUtility.labelWidth + 2f,
                position.y,
                position.width - EditorGUIUtility.labelWidth - 2f,
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.HelpBox(valueRect, $"Property type is null", MessageType.Warning);
        }

        public static float GetNullPropertyHeight() {
            return EditorGUIUtility.singleLineHeight;
        }

        public static VisualElement CreateBlackboardPropertyView(
            SerializedBlackboardProperty property,
            Action onPropertyValueChanged
        ) {
            var type = (Type) property.blackboardProperty.type;
            string typeName = TypeNameFormatter.GetTypeName(type);
            string propertyName = property.blackboardProperty.name;
            var nameField = new BlackboardField { text = propertyName, typeText = typeName };

            var valueField = type == null
                ? new VisualElement()
                : CreateBlackboardPropertyField(property, onPropertyValueChanged);

            var container = new VisualElement();
            container.Add(nameField);
            container.Add(new BlackboardRow(nameField, valueField) { expanded = true });
            return container;
        }

        private static VisualElement CreateBlackboardPropertyField(SerializedBlackboardProperty property, Action onPropertyValueChanged) {
            var type = (Type) property.blackboardProperty.type;
            object currentValue = property.serializedProperty.GetValue();

            var propertyField = new PropertyField {
                bindingPath = property.serializedProperty.propertyPath,
                label = string.Empty
            };

            propertyField.Bind(property.serializedProperty.serializedObject);
            propertyField.RegisterValueChangeCallback(e => {
                object value = e.changedProperty.GetValue();

                if (type.IsEnum &&
                    value is BlackboardValue<long> longValue &&
                    currentValue is BlackboardValue<long> longCurrentValue &&
                    longValue == longCurrentValue
                ) {
                    return;
                }

                if (type.IsValueType && Equals(value, currentValue)) return;

                if (type.IsAssignableFrom(typeof(Object)) &&
                    value is BlackboardValue<Object> objectValue &&
                    currentValue is BlackboardValue<Object> objectCurrentValue &&
                    objectValue == objectCurrentValue
                ) {
                    return;
                }

                if (type.IsInterface || type.IsSubclassOf(typeof(object)) && Equals(value, currentValue)) return;

                currentValue = value;
                onPropertyValueChanged.Invoke();
            });

            return propertyField;
        }

        private const string BOOLS = "_bools";
        private const string INTS = "_ints";
        private const string LONGS = "_longs";
        private const string FLOATS = "_floats";
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
        private const string ENUMS = "_enums";
        private const string REFERENCES = "_references";

        private const string VALUE = "value";

        private const string ENTRIES = "_entries";
        private const string KEY = "key";

        public static List<SerializedBlackboardProperty> GetSerializedBlackboardProperties(SerializedProperty blackboardSerializedProperty) {
            var blackboard = (Blackboard) blackboardSerializedProperty.GetValue();
            int propertiesCount = blackboard.Properties.Count;

            var propertiesMap = new Dictionary<int, (int index, BlackboardProperty property)>(propertiesCount);
            var properties = new List<SerializedBlackboardProperty>(propertiesCount);

            for (int i = 0; i < propertiesCount; i++) {
                int hash = blackboard.Properties[i];
                blackboard.TryGetProperty(hash, out var property);

                propertiesMap[hash] = (i, property);
                properties.Add(default);
            }

            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(BOOLS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(INTS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(LONGS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(FLOATS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(DOUBLES), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(STRINGS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(VECTORS2), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(VECTORS3), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(VECTORS4), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(VECTORS2_INT), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(VECTORS3_INT), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(QUATERNIONS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(COLORS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(LAYER_MASKS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(CURVES), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(OBJECTS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(ENUMS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(REFERENCES), propertiesMap, properties);

            return properties;
        }

        private static void FetchBlackboardDictionary(
            SerializedProperty dictionary,
            IReadOnlyDictionary<int, (int index, BlackboardProperty property)> propertiesMap,
            IList<SerializedBlackboardProperty> dest
        ) {
            var entries = dictionary.FindPropertyRelative(ENTRIES);

            for (int e = 0; e < entries.arraySize; e++) {
                var entry = entries.GetArrayElementAtIndex(e);
                int hash = entry.FindPropertyRelative(KEY).intValue;

                if (!propertiesMap.TryGetValue(hash, out var p)) continue;

                dest[p.index] = new SerializedBlackboardProperty(p.property, entry.FindPropertyRelative(VALUE));
            }
        }
    }

}
