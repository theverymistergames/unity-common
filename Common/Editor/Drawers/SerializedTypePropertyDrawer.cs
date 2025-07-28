using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Common.Attributes;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Types;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(SerializedType))]
    public class SerializedTypePropertyDrawer : PropertyDrawer {

        private const string EDITOR = "editor";
        private const string NULL = "null";
        
        private static GUIContent NullLabel => new GUIContent("null");
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            string typeString = property.FindPropertyRelative("_type").stringValue;
            
            var type = SerializedType.DeserializeType(typeString);
            var filters = fieldInfo.GetCustomAttributes<TypeFilterAttribute>().ToArray();

            if (filters.Length == 0) {
                EditorGUI.LabelField(position, label, new GUIContent(TypeNameFormatter.GetShortTypeName(type)));
                return;
            }
            
            var typeLabel = type == null ? NullLabel : new GUIContent(TypeNameFormatter.GetShortTypeName(type));
            label = label == null || label.text == typeString ? GUIContent.none : label;
            
            float popupWidth = label == GUIContent.none || string.IsNullOrEmpty(label.text)
                ? position.width - 14f
                : position.width - EditorGUIUtility.labelWidth;

            var popupPosition = new Rect(position.x + position.width - popupWidth, position.y, popupWidth, EditorGUIUtility.singleLineHeight);
            
            if (EditorGUI.DropdownButton(popupPosition, typeLabel, FocusType.Keyboard)) {
                CreateTypeDropdown(property, filters).Show(popupPosition);
            }

            EditorGUI.LabelField(position, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
        
        private static AdvancedDropdown<Type> CreateTypeDropdown(SerializedProperty property, TypeFilterAttribute[] filters) {
            var types = new List<Type> { null };
            
            for (int i = 0; i < filters.Length; i++) {
                var attr = filters[i];
                if (string.IsNullOrWhiteSpace(attr.propertyName)) continue;
                
                string path = SerializedPropertyExtensions.GetNeighbourPropertyPath(property, attr.propertyName);
                var prop = property.serializedObject.FindProperty(path);

                if (prop?.GetValue() is not {} value) continue;

                types.AddRange(GetAllParentTypesAndInterfaces(value.GetType(), attr.mode));
            }

            property = property.Copy();

            return new AdvancedDropdown<Type>(
                "Select type",
                types,
                t => t == null ? NULL : t.FullName,
                (t, _) => SetSerializedType(t, property),
                separator: '.',
                sort: children => children
                    .OrderByDescending(n => n.data.data == null && n.children.Count == 0)
                    .ThenBy(n => n.children.Count == 0)
                    .ThenBy(n => n.data.name)
            );
        }
        
        private static void SetSerializedType(Type type, SerializedProperty property) {
            string serializedType = type == null ? null : SerializedType.SerializeType(type);

            property.FindPropertyRelative("_type").stringValue = serializedType;

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }
        
        private static bool IsSupportedType(Type t) {
            return (t.IsPublic || t.IsNestedPublic) && !t.IsGenericType &&
                   t.FullName is not null && !t.FullName.Contains(EDITOR, StringComparison.OrdinalIgnoreCase);
        }
        
        private static List<Type> GetAllParentTypesAndInterfaces(Type type, TypeFilterMode mode) {
            var parentTypes = new List<Type>();
            var currentType = type.BaseType;

            if ((mode & TypeFilterMode.Classes) == TypeFilterMode.Classes || 
                (mode & TypeFilterMode.ValueTypes) == TypeFilterMode.ValueTypes) 
            {
                if ((mode & TypeFilterMode.ExcludeSelf) == 0 &&
                    ((mode & TypeFilterMode.Classes) == TypeFilterMode.Classes && type.IsClass && !type.IsValueType ||
                    (mode & TypeFilterMode.ValueTypes) == TypeFilterMode.ValueTypes && type.IsValueType)) 
                {
                    parentTypes.Add(type);
                }
                
                while (currentType != null && currentType != typeof(object)) {
                    if ((mode & TypeFilterMode.Classes) == TypeFilterMode.Classes && 
                        currentType.IsClass && !currentType.IsValueType && IsSupportedType(currentType) || 
                        (mode & TypeFilterMode.ValueTypes) == TypeFilterMode.ValueTypes && 
                        currentType.IsValueType && IsSupportedType(currentType)) 
                    {
                        parentTypes.Add(currentType);
                    }
                    
                    currentType = currentType.BaseType;
                }
            }

            if ((mode & TypeFilterMode.Interfaces) == TypeFilterMode.Interfaces) {
                parentTypes.AddRange(type.GetInterfaces().Where(IsSupportedType));
                
                if ((mode & TypeFilterMode.ExcludeSelf) == 0 && type.IsInterface) parentTypes.Add(type);
            }
            
            return parentTypes;
        }
    }

}
