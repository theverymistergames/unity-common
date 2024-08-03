using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Labels;
using MisterGames.Common.Lists;
using MisterGames.Common.Strings;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(LabelValue))]
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
            
            public readonly LabelLibrary library;
            public readonly int array;
            public readonly int value;
            public readonly int index;
            public readonly string path;

            public Entry(LabelLibrary library, int array, int value, int index, string path) {
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

            var library = libraryProperty.objectReferenceValue as LabelLibrary;
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
            
            return AssetDatabase
                .FindAssets($"a:assets t:{nameof(LabelLibrary)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<LabelLibrary>(AssetDatabase.GUIDToAssetPath(guid)))
                .SelectMany(lib => GetLibraryEntries(lib, filters))
                .Prepend(default);
        }

        private static IEnumerable<Entry> GetLibraryEntries(LabelLibrary library, LabelFilterAttribute[] filters) {
            if (library == null || library.labelArrays?.Length <= 0) return Array.Empty<Entry>();

            string libName = library.name;
            var list = new List<Entry>();

            var labelArrays = library.labelArrays;
            for (int i = 0; i < labelArrays!.Length; i++) {
                var labelArray = labelArrays[i];
                string arrayName = string.IsNullOrWhiteSpace(labelArray.name)
                    ? labelArrays.Length == 1 ? null : $"Array [{i}]"
                    : labelArray.name;

                string path;
                
                if (labelArray.none) {
                    path = GetEntryPath(libName, arrayName, None);
                    if (!IsValidPath(path, filters)) continue;
                    
                    list.Add(new Entry(library, i, 0, 0, path));
                }

                int indexOffset = labelArray.none ? 1 : 0;
                for (int j = 0; j < labelArray.labels?.Length; j++) {
                    path = GetEntryPath(libName, arrayName, labelArray.labels[j]);
                    if (!IsValidPath(path, filters)) continue;
                    
                    int value = labelArray.usage switch {
                        LabelLibrary.Usage.ByIndex => j + indexOffset,
                        LabelLibrary.Usage.ByHash => Animator.StringToHash(labelArray.labels[j]),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    
                    list.Add(new Entry(library, i, value, j + indexOffset, path));
                }
            }

            return list;
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

        private static GUIContent GetDropdownLabel(LabelLibrary library, int array, int value) {
            if (library == null) return NullLabel;

            var labelArrays = library.labelArrays ?? Array.Empty<LabelLibrary.LabelArray>();
            if (array < 0 || array >= labelArrays.Length) {
                return new GUIContent($"{library.name}{Separator}Array [{array}]{Separator}Value [{value}] {NotFound}");
            }

            var labelArray = labelArrays[array];
            
            string arrayName = string.IsNullOrWhiteSpace(labelArray.name) 
                ? labelArrays.Length == 1 
                    ? Separator 
                    : $"{Separator}Array [{array}]{Separator}" 
                : $"{Separator}{labelArray.name}{Separator}";
            
            if (labelArray.none && value == 0) {
                return new GUIContent($"{library.name}{arrayName}{None}");
            }

            switch (labelArray.usage) {
                case LabelLibrary.Usage.ByIndex:
                    int count = (labelArray.none ? 1 : 0) + (labelArray.labels?.Length ?? 0);
                    int index = labelArray.none ? value - 1 : value;
                    
                    return value >= 0 && value < count 
                        ? string.IsNullOrWhiteSpace(labelArray.labels![index]) 
                            ? new GUIContent($"{library.name}{arrayName}Value [{value}]") 
                            : new GUIContent($"{library.name}{arrayName}{labelArray.labels![index]}") 
                        : new GUIContent($"{library.name}{arrayName}Value [{value}] {NotFound}");
                
                case LabelLibrary.Usage.ByHash:
                    return labelArray.labels?.TryFind(value, (s, v) => Animator.StringToHash(s) == v, out string label) ?? false 
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