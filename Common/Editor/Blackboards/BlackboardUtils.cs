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

            var nameField = new BlackboardField { text = property.blackboardProperty.name, typeText = typeName };

            VisualElement valueField;
            if (typeof(Object).IsAssignableFrom(property.blackboardProperty.type)) {
                var objectField = new ObjectField();

                objectField.bindingPath = property.serializedProperty.propertyPath;
                objectField.allowSceneObjects = false;
                objectField.objectType = property.blackboardProperty.type;
                objectField.label = string.Empty;

                objectField.Bind(property.serializedProperty.serializedObject);
                objectField.RegisterValueChangedCallback(_ => onChanged.Invoke());

                valueField = objectField;
            }
            else {
                var propertyField = new PropertyField();

                propertyField.bindingPath = property.serializedProperty.propertyPath;
                propertyField.label = string.Empty;

                propertyField.Bind(property.serializedProperty.serializedObject);
                propertyField.RegisterValueChangeCallback(_ => onChanged.Invoke());

                valueField = propertyField;
            }

            var row = new BlackboardRow(nameField, valueField);

            var container = new VisualElement();
            container.Add(nameField);
            container.Add(row);

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
