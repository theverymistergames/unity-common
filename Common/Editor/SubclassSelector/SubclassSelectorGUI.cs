using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Editor.Drawers {

    public static class SubclassSelectorGUI {

        private const string NULL = "null";
        private const string EDITOR = "editor";

        private static readonly Type UnityObjectType = typeof(Object);

        private static GUIContent NullLabel => new GUIContent("null");
        private static GUIContent IsNotManagedReferenceLabel => new GUIContent("Property type is not a managed reference");
        private static GUIContent PropertyTypeNullLabel => new GUIContent("Property type is null");
        private static GUIContent PropertyTypeUnsupportedLabel => new GUIContent("Property type is unsupported");

        public static void PropertyField(Rect position, SerializedProperty property, Type baseType, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.ManagedReference) {
                EditorGUI.LabelField(position, label, IsNotManagedReferenceLabel);
                return;
            }

            if (baseType == null) {
                EditorGUI.LabelField(position, label, PropertyTypeNullLabel);
                return;
            }

            if (!IsSupportedBaseType(baseType)) {
                EditorGUI.LabelField(position, label, PropertyTypeUnsupportedLabel);
                return;
            }

            var type = GetManagedReferenceValueType(property);
            var typeLabel = type == null ? NullLabel : new GUIContent(type.Name);

            float popupWidth = label == GUIContent.none || string.IsNullOrEmpty(label.text)
                ? position.width - 14f
                : position.width - EditorGUIUtility.labelWidth;

            var popupPosition = new Rect(position.x + position.width - popupWidth, position.y, popupWidth, EditorGUIUtility.singleLineHeight);

            if (EditorGUI.DropdownButton(popupPosition, typeLabel, FocusType.Keyboard)) {
                CreateTypeDropdown(baseType, property).Show(popupPosition);
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        public static float GetPropertyHeight(SerializedProperty property, bool includeChildren = false) {
            return EditorGUI.GetPropertyHeight(property, includeChildren);
        }

        private static AdvancedDropdown<Type> CreateTypeDropdown(Type baseType, SerializedProperty property) {
            var types = TypeCache
                .GetTypesDerivedFrom(baseType)
                .Append(baseType)
                .Where(IsSupportedType)
                .Append(null);

            return new AdvancedDropdown<Type>(
                "Select type",
                types,
                t => t == null ? NULL : t.FullName,
                t => {
                    object value = t == null ? null : Activator.CreateInstance(t);

                    property.managedReferenceValue = value;
                    property.isExpanded = value != null;

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                },
                separator: '.',
                sort: children => children
                    .OrderByDescending(n => n.data.data == null && n.children.Count == 0)
                    .ThenBy(n => n.children.Count == 0)
                    .ThenBy(n => n.data.name)
            );
        }

        private static bool IsSupportedType(Type t) {
            return (t.IsPublic || t.IsNestedPublic) && !t.IsAbstract && !t.IsGenericType && !t.IsValueType &&
                   t.FullName is not null && !t.FullName.Contains(EDITOR, StringComparison.OrdinalIgnoreCase) &&
                   !UnityObjectType.IsAssignableFrom(t) && Attribute.IsDefined(t, typeof(SerializableAttribute));
        }

        private static bool IsSupportedBaseType(Type t) {
            return (t.IsPublic || t.IsNestedPublic) && !t.IsGenericType && !t.IsValueType &&
                   t.FullName is not null && !t.FullName.Contains(EDITOR, StringComparison.OrdinalIgnoreCase) &&
                   !UnityObjectType.IsAssignableFrom(t) && (t.IsInterface || t.IsAbstract || Attribute.IsDefined(t, typeof(SerializableAttribute)));
        }

        private static Type GetManagedReferenceValueType(SerializedProperty property) {
            string typeName = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(typeName)) return null;

            int splitIndex = typeName.IndexOf(' ');
            return Assembly.Load(typeName[..splitIndex]).GetType(typeName[(splitIndex + 1)..]);
        }
    }

}
