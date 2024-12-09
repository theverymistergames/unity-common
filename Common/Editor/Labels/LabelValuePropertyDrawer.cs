using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Labels;
using MisterGames.Common.Strings;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(LabelValue))]
    [CustomPropertyDrawer(typeof(LabelValue<>))]
    public sealed class LabelValuePropertyDrawer : PropertyDrawer {
        
        private const string Separator = " : ";
        private const string None = "None";
        private const string Null = "<null>";
        private const string NotFound = "(not found)";
        
        private const string LibraryPropertyPath = "library";
        private const string ArrayPropertyPath = "array";
        private const string ValuePropertyPath = "value";

        private static readonly GUIContent NullLabel = new(Null);
        
        private readonly struct Entry {
            
            public readonly LabelLibraryBase library;
            public readonly int array;
            public readonly int value;
            public readonly int index;
            public readonly string path;

            public Entry(LabelLibraryBase library, int array, int value, int index, string path) {
                this.library = library;
                this.array = array;
                this.value = value;
                this.index = index;
                this.path = path;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            
            var rect = position;
            float indent = EditorGUI.indentLevel * 15;
            float offset = indent - 1f;
            rect.x += offset;
            rect.width -= offset;
            
            GUI.Label(rect, label);

            var libraryProperty = property.FindPropertyRelative(LibraryPropertyPath); 
            var arrayProperty = property.FindPropertyRelative(ArrayPropertyPath); 
            var valueProperty = property.FindPropertyRelative(ValuePropertyPath);

            var library = libraryProperty.objectReferenceValue as LabelLibraryBase;
            int array = arrayProperty.intValue;
            int value = valueProperty.intValue;

            if (library == null) {
                array = -1;
                arrayProperty.intValue = -1;
                
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            rect = position;
            offset = EditorGUIUtility.labelWidth + 2f;
            rect.x += offset;
            rect.width -= offset;
            
            if (EditorGUI.DropdownButton(rect, GetDropdownLabel(library, array, value), FocusType.Keyboard)) {
                var dropdown = new AdvancedDropdown<Entry>(
                    "Select value",
                    GetAllEntries(fieldInfo),
                    e => e.path ?? Null,
                    onItemSelected: e => {
                        var p = property.Copy();
                        
                        property.FindPropertyRelative(LibraryPropertyPath).objectReferenceValue = e.library; 
                        property.FindPropertyRelative(ArrayPropertyPath).intValue = e.array; 
                        property.FindPropertyRelative(ValuePropertyPath).intValue = e.value;
                        
                        p.serializedObject.ApplyModifiedProperties();
                        p.serializedObject.Update();
                    },
                    sort: nodes => nodes
                        .OrderBy(n => n.data.data.library == null)
                        .ThenBy(n => n.data.data.index)
                        .ThenBy(n => n.data.name)
                );
                
                dropdown.Show(rect);
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        private static IEnumerable<Entry> GetAllEntries(FieldInfo propertyFieldInfo) {
            var filters = propertyFieldInfo.GetCustomAttributes<LabelFilterAttribute>().ToArray();
            var genericType = propertyFieldInfo.FieldType.IsGenericType ? propertyFieldInfo.FieldType.GetGenericArguments()[0] : null;
            
            return AssetDatabase
                .FindAssets($"a:assets t:{nameof(LabelLibraryBase)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<LabelLibraryBase>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(lib => IsValidLibraryType(lib, genericType))
                .SelectMany(lib => GetLibraryEntries(lib, filters))
                .Prepend(default);
        }

        private static IEnumerable<Entry> GetLibraryEntries(LabelLibraryBase library, LabelFilterAttribute[] filters) {
            int arrayCount = library == null ? 0 : library.GetArraysCount();
            if (arrayCount <= 0) return Array.Empty<Entry>();

            string libName = library.name;
            var list = new List<Entry>();

            for (int i = 0; i < arrayCount; i++) {
                string arrayName = library.GetArrayName(i);
                arrayName = string.IsNullOrWhiteSpace(arrayName)
                    ? arrayCount == 1 ? null : $"Array [{i}]"
                    : arrayName;

                string path;
                bool none = library.GetArrayNoneLabel(i);
                
                if (none) {
                    path = GetEntryPath(libName, arrayName, None);
                    if (!IsValidPath(path, filters)) continue;
                    
                    list.Add(new Entry(library, i, 0, 0, path));
                }

                int indexOffset = none ? 1 : 0;
                int labelsCount = library.GetLabelsCount(i);
                var usage = library.GetArrayUsage(i);
                
                for (int j = 0; j < labelsCount; j++) {
                    string label = library.GetLabelByIndex(i, j);
                    path = GetEntryPath(libName, arrayName, label);
                    
                    if (!IsValidPath(path, filters)) continue;
                    
                    int value = usage switch {
                        LabelArrayUsage.ByIndex => j + indexOffset,
                        LabelArrayUsage.ByHash => Animator.StringToHash(label),
                        _ => throw new ArgumentOutOfRangeException(),
                    };
                    
                    list.Add(new Entry(library, i, value, j + indexOffset, path));
                }
            }

            return list;
        }

        private static bool IsValidLibraryType(LabelLibraryBase library, Type genericType) {
            var dataType = library.GetDataType();
            return genericType?.IsAssignableFrom(dataType) ?? dataType == null;
        }
        
        private static bool IsValidPath(string path, LabelFilterAttribute[] filters) {
            if (filters.Length == 0 || string.IsNullOrEmpty(path)) return true;
            
            for (int i = 0; i < filters.Length; i++) {
                string filterPath = filters[i].path ?? string.Empty;
                if (path.IsSubPathOf(filterPath, '/')) return true;
            }

            return false;
        }

        private static string GetEntryPath(string library, string array, string value) {
            return string.IsNullOrWhiteSpace(array)
                    ? $"{library}/{value}"
                    : $"{library}/{array}/{value}";
        }

        private static GUIContent GetDropdownLabel(LabelLibraryBase library, int array, int value) {
            if (library == null) return NullLabel;

            int arrayCount = library.GetArraysCount();
            if (array < 0 || array >= arrayCount) {
                return new GUIContent($"{library.name}{Separator}Array [{array}]{Separator}Value [{value}] {NotFound}");
            }

            string arrayName = library.GetArrayName(array);
            arrayName = string.IsNullOrWhiteSpace(arrayName) 
                ? arrayCount == 1 
                    ? Separator 
                    : $"{Separator}Array [{array}]{Separator}" 
                : $"{Separator}{arrayName}{Separator}";

            bool none = library.GetArrayNoneLabel(array);
            
            if (none && value == 0) {
                return new GUIContent($"{library.name}{arrayName}{None}");
            }

            int labelsCount = library.GetLabelsCount(array);
            string label = library.GetLabel(array, value);
            bool containsLabel = library.ContainsLabel(array, value);
            
            switch (library.GetArrayUsage(array)) {
                case LabelArrayUsage.ByIndex:
                    int count = (none ? 1 : 0) + labelsCount;
                    
                    return value >= 0 && value < count 
                        ? string.IsNullOrWhiteSpace(label) 
                            ? new GUIContent($"{library.name}{arrayName}Value [{value}]") 
                            : new GUIContent($"{library.name}{arrayName}{label}") 
                        : new GUIContent($"{library.name}{arrayName}Value [{value}] {NotFound}");
                
                case LabelArrayUsage.ByHash:
                    return containsLabel 
                        ? string.IsNullOrWhiteSpace(label) 
                            ? new GUIContent($"{library.name}{arrayName}Value [hash {value}]") 
                            : new GUIContent($"{library.name}{arrayName}{label}") 
                        : new GUIContent($"{library.name}{arrayName}Value [hash {value}] {NotFound}");
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
}