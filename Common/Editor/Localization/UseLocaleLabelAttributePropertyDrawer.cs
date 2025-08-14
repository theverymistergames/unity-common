using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Common.Localization;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes {

    [CustomPropertyDrawer(typeof(UseLocaleLabelAttribute))]
    public sealed class UseLocaleLabelAttributePropertyDrawer : PropertyDrawer {

        private const string Null = "<null>";
        private const string NotDefined = "<not defined>";
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (label != null && label != GUIContent.none && 
                property.serializedObject.targetObject is LocalizationTableStorageBase table && 
                SerializedPropertyExtensions.TryGetPropertyIndexInArray(property, out int index)) 
            {
                int count = table.GetLocaleCount();
                label.text = index < count
                    ? GetLocaleLabel(table.GetLocale(index).GetDescriptor())
                    : NotDefined;
            }
            
            CustomPropertyGUI.PropertyField(position, property, label, fieldInfo, attribute, includeChildren: true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return CustomPropertyGUI.GetPropertyHeight(property, label, fieldInfo, attribute, includeChildren: true);
        }
        
        private static string GetLocaleLabel(LocaleDescriptor localeDescriptor) {
            return string.IsNullOrWhiteSpace(localeDescriptor.code)
                ? Null
                : $"{localeDescriptor.code} ({localeDescriptor.description})";
        }
    }

}
