using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Common.Maths;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes {

    [CustomPropertyDrawer(typeof(VisibleIfAttribute))]
    public sealed class VisibleIfDrawer : PropertyDrawer {

        private bool _hasCachedPropertyDrawer;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!Show(property)) return;
            
            CustomPropertyGUI.PropertyField(position, property, label, fieldInfo, attribute, includeChildren: true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return Show(property) 
                ? CustomPropertyGUI.GetPropertyHeight(property, label, fieldInfo, attribute, includeChildren: true) 
                : 0f;
        }

        private bool Show(SerializedProperty property) {
            var attr = (VisibleIfAttribute) attribute;
            
            string propertyPath = GetNeighbourPropertyPath(property, attr.property);
            var valueProperty = property.serializedObject.FindProperty(propertyPath);

            int value = valueProperty switch {
                {propertyType: SerializedPropertyType.ObjectReference} => (valueProperty.objectReferenceValue != null).AsInt(),
                {propertyType: SerializedPropertyType.ManagedReference} => (valueProperty.managedReferenceValue != null).AsInt(),
                {propertyType: SerializedPropertyType.Boolean} => valueProperty.boolValue.AsInt(),
                {propertyType: SerializedPropertyType.Enum or SerializedPropertyType.Integer} => valueProperty.intValue,
                _ => 0
            };

            return Match(value, attr.value, attr.mode);
        }

        private static bool Match(int a, int b, CompareMode mode) {
            return mode switch {
                CompareMode.Equal => a == b,
                CompareMode.NotEqual => a != b,
                CompareMode.Greater => a > b,
                CompareMode.Less => a < b,
                CompareMode.GreaterOrEqual => a >= b,
                CompareMode.LessOrEqual => a <= b,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        private static string GetNeighbourPropertyPath(SerializedProperty property, string propertyName) {
            string neighbourPath = property.propertyPath;
            int dotIndex = neighbourPath.LastIndexOf('.');
            return dotIndex < 0 ? propertyName : $"{neighbourPath.Remove(dotIndex)}.{propertyName}";
        }
    }

}
