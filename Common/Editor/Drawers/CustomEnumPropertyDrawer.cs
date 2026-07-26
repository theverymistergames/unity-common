using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(Enum), true)]
    public sealed class AdvancedEnumDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.Enum) {
                EditorGUI.LabelField(position, label, new GUIContent("Expected enum"));
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            try {
                var enumType = GetEnumType(fieldInfo);

                // AdvancedDropdown is single-select, so preserve normal Flags behavior.
                if (enumType is { IsEnum: true } &&
                    enumType.IsDefined(typeof(FlagsAttribute), false)) {
                    DrawFlags(position, label, property, enumType);
                    return;
                }

                DrawRegularEnum(position, label, property);
            }
            finally {
                EditorGUI.EndProperty();
            }
        }

        private static void DrawRegularEnum(Rect position, GUIContent label, SerializedProperty property) {
            var fieldRect = EditorGUI.PrefixLabel(position, label);

            bool previousMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            try {
                string[] names = property.enumDisplayNames;
                int index = property.enumValueIndex;

                string displayName;

                if (property.hasMultipleDifferentValues)
                    displayName = "—";
                else if (index >= 0 && index < names.Length)
                    displayName = names[index];
                else
                    displayName = "Unknown";

                using (new EditorGUI.DisabledScope(!property.editable)) {
                    if (GUI.Button(fieldRect, displayName, EditorStyles.popup))
                        new EnumDropdown(property).Show(fieldRect);
                }
            }
            finally {
                EditorGUI.showMixedValue = previousMixedValue;
            }
        }

        private static void DrawFlags(Rect position, GUIContent label, SerializedProperty property, Type enumType) {
            bool previousMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            try {
                EditorGUI.BeginChangeCheck();

                var currentValue = (Enum) Enum.ToObject(
                    enumType,
                    property.enumValueFlag);

                var newValue = EditorGUI.EnumFlagsField(
                    position,
                    label,
                    currentValue);

                if (EditorGUI.EndChangeCheck())
                    property.enumValueFlag = Convert.ToInt32(newValue);
            }
            finally {
                EditorGUI.showMixedValue = previousMixedValue;
            }
        }

        private static Type GetEnumType(FieldInfo field) {
            if (field == null)
                return null;

            var type = field.FieldType;

            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];

            return type;
        }

        private sealed class EnumDropdown : AdvancedDropdown {
            
            private readonly SerializedObject serializedObject;
            private readonly string propertyPath;
            private readonly string[] displayNames;

            public EnumDropdown(SerializedProperty property)
                : base(new AdvancedDropdownState()) {
                serializedObject = property.serializedObject;
                propertyPath = property.propertyPath;
                displayNames = (string[]) property.enumDisplayNames.Clone();

                minimumSize = new Vector2(260f, 300f);
            }

            protected override AdvancedDropdownItem BuildRoot() {
                var root = new AdvancedDropdownItem("Select Enum");
                var groups = new Dictionary<string, AdvancedDropdownItem>();

                for (int enumIndex = 0; enumIndex < displayNames.Length; enumIndex++) {
                    string[] parts = displayNames[enumIndex].Split('/');
                    var parent = root;
                    string groupPath = string.Empty;

                    // Create category nodes for names such as "Keyboard/Space".
                    for (int partIndex = 0; partIndex < parts.Length - 1; partIndex++) {
                        string groupName = parts[partIndex].Trim();

                        if (string.IsNullOrEmpty(groupName))
                            continue;

                        groupPath = string.IsNullOrEmpty(groupPath)
                            ? groupName
                            : groupPath + "/" + groupName;

                        if (!groups.TryGetValue(groupPath, out var group)) {
                            group = new AdvancedDropdownItem(groupName);
                            groups.Add(groupPath, group);
                            parent.AddChild(group);
                        }

                        parent = group;
                    }

                    string leafName = parts[^1].Trim();

                    if (string.IsNullOrEmpty(leafName))
                        leafName = displayNames[enumIndex];

                    parent.AddChild(new EnumOptionItem(leafName, enumIndex));
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item) {
                // Category entries are not valid enum selections.
                if (item is not EnumOptionItem enumItem)
                    return;

                serializedObject.Update();

                var property = serializedObject.FindProperty(propertyPath);
                if (property is not { propertyType: SerializedPropertyType.Enum })
                    return;

                property.enumValueIndex = enumItem.EnumIndex;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private sealed class EnumOptionItem : AdvancedDropdownItem {
            public readonly int EnumIndex;

            public EnumOptionItem(string name, int enumIndex)
                : base(name) {
                EnumIndex = enumIndex;
            }
        }
    }
    
}