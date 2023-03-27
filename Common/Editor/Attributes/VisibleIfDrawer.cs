using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(VisibleIfAttribute))]
    public class VisibleIfDrawer : PropertyDrawer {

        private bool _hasCachedPropertyDrawer;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var visibleIf = (VisibleIfAttribute) attribute;
            string boolPropertyPath = GetNeighbourPropertyPath(property, visibleIf.boolPropertyName);

            if (!string.IsNullOrEmpty(boolPropertyPath) &&
                property.serializedObject.FindProperty(boolPropertyPath) is { propertyType: SerializedPropertyType.Boolean, boolValue: false }
            ) {
                return;
            }

            PropertyDrawerUtils.DrawPropertyField(position, property, label, fieldInfo, attribute, includeChildren: true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var visibleIf = (VisibleIfAttribute) attribute;
            string boolPropertyPath = GetNeighbourPropertyPath(property, visibleIf.boolPropertyName);

            if (!string.IsNullOrEmpty(boolPropertyPath) &&
                property.serializedObject.FindProperty(boolPropertyPath) is { propertyType: SerializedPropertyType.Boolean, boolValue: false }
            ) {
                return 0f;
            }

            return PropertyDrawerUtils.GetPropertyHeight(property, label, fieldInfo, attribute, includeChildren: true);
        }

        private static string GetNeighbourPropertyPath(SerializedProperty property, string propertyName) {
            string neighbourPath = property.propertyPath;
            int dotIndex = neighbourPath.LastIndexOf('.');
            return dotIndex < 0 ? propertyName : $"{neighbourPath.Remove(dotIndex)}.{propertyName}";
        }
    }

}
