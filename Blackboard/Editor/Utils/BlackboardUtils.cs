using System;
using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Blackboards.Core.Blackboard;

namespace MisterGames.Blackboards.Editor {

    public static class BlackboardUtils {

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

        private const string BOOL_ARRAYS = "_boolArrays";
        private const string INT_ARRAYS = "_intArrays";
        private const string LONG_ARRAYS = "_longArrays";
        private const string FLOAT_ARRAYS = "_floatArrays";
        private const string DOUBLE_ARRAYS = "_doubleArrays";
        private const string STRING_ARRAYS = "_stringArrays";
        private const string VECTORS2_ARRAYS = "_vectors2Arrays";
        private const string VECTORS3_ARRAYS = "_vectors3Arrays";
        private const string VECTORS4_ARRAYS = "_vectors4Arrays";
        private const string VECTORS2_INT_ARRAYS = "_vectors2IntArrays";
        private const string VECTORS3_INT_ARRAYS = "_vectors3IntArrays";
        private const string QUATERNION_ARRAYS = "_quaternionArrays";
        private const string COLOR_ARRAYS = "_colorArrays";
        private const string LAYER_MASK_ARRAYS = "_layerMaskArrays";
        private const string CURVE_ARRAYS = "_curveArrays";
        private const string OBJECT_ARRAYS = "_objectArrays";
        private const string ENUM_ARRAYS = "_enumArrays";
        private const string REFERENCE_ARRAYS = "_referenceArrays";

        private const string ENTRIES = "_entries";
        private const string KEY = "key";
        private const string VALUE = "value";

        public static bool TryGetBlackboardProperty(
            SerializedProperty p,
            out BlackboardProperty blackboardProperty,
            out Blackboard blackboard,
            out int hash
        ) {
            blackboardProperty = default;
            blackboard = null;
            hash = 0;

            string path = p.propertyPath;
            Span<string> pathParts = path.Split('.');

            for (int i = pathParts.Length - 1; i > 0; i--) {
                string pathPart = pathParts[i];

                int pathPartLength = pathPart.Length;
                path = path.Remove(path.Length - pathPartLength - 1, pathPartLength + 1);

                if (pathPart != "value") continue;

                var hashProperty = p.serializedObject.FindProperty($"{path}.key");
                if (hashProperty is not { propertyType: SerializedPropertyType.Integer }) continue;

                if (i < 5) return false;

                string pathCache = path;

                for (int j = i - 1; j > i - 5; j--) {
                    pathPart = pathParts[j];
                    pathPartLength = pathPart.Length;

                    path = path.Remove(path.Length - pathPartLength - 1, pathPartLength + 1);
                }

                blackboard = p.serializedObject.FindProperty(path)?.GetValue() as Blackboard;

                if (blackboard == null) {
                    path = pathCache;
                    continue;
                }

                hash = hashProperty.intValue;
                break;
            }

            return blackboard != null && blackboard.TryGetProperty(hash, out blackboardProperty) && blackboardProperty.type != null;
        }

        public static VisualElement CreateBlackboardPropertyView(SerializedBlackboardProperty property) {
            var type = (Type) property.blackboardProperty.type;
            string typeName = TypeNameFormatter.GetTypeName(type);
            string propertyName = property.blackboardProperty.name;

            return new BlackboardField { text = propertyName, typeText = typeName };
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

            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(BOOL_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(INT_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(LONG_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(FLOAT_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(DOUBLE_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(STRING_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(VECTORS2_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(VECTORS3_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(VECTORS4_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(VECTORS2_INT_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(VECTORS3_INT_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(QUATERNION_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(COLOR_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(LAYER_MASK_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(CURVE_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(OBJECT_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(ENUM_ARRAYS), propertiesMap, properties);
            FetchBlackboardDictionary(blackboardSerializedProperty.FindPropertyRelative(REFERENCE_ARRAYS), propertiesMap, properties);

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
