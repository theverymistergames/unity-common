using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Labels;
using MisterGames.Common.Labels.Base;
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

        private const string LibraryPropertyPath = nameof(LabelValue.library);
        private const string IdPropertyPath = nameof(LabelValue.id);

        private static readonly GUIContent NullLabel = new(Null);
        
        private readonly struct Entry {
            
            public readonly LabelLibraryBase library;
            public readonly int array;
            public readonly int id;
            public readonly int index;
            public readonly LabelArrayUsage usage;
            public readonly string path;

            public Entry(LabelLibraryBase library, int id, int index, int array, string path, LabelArrayUsage usage) {
                this.library = library;
                this.id = id;
                this.index = index;
                this.array = array;
                this.path = path;
                this.usage = usage;
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

            rect = position;
            offset = EditorGUIUtility.labelWidth + 2f;
            rect.x += offset;
            rect.width -= offset;
            
            var libraryProperty = property.FindPropertyRelative(LibraryPropertyPath); 
            var idProperty = property.FindPropertyRelative(IdPropertyPath);

            if (libraryProperty == null || idProperty == null) {
                GUI.Label(rect, $"Error: {property.propertyPath} is not a LabelValue");
                EditorGUI.EndProperty();
                return;
            }
            
            var library = libraryProperty.objectReferenceValue as LabelLibraryBase;
            int id = idProperty.intValue;

            if (library == null) {
                id = 0;
                idProperty.intValue = id;
                
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }
            
            if (EditorGUI.DropdownButton(rect, GetDropdownLabel(library, id), FocusType.Keyboard)) {
               var dropdown = new AdvancedDropdown<Entry>(
                    "Select value",
                    GetAllEntries(fieldInfo),
                    e => e.path ?? Null,
                    onItemSelected: (e, _) => {
                        var p = property.Copy();
                        
                        property.FindPropertyRelative(LibraryPropertyPath).objectReferenceValue = e.library; 
                        property.FindPropertyRelative(IdPropertyPath).intValue = e.id; 
                        
                        p.serializedObject.ApplyModifiedProperties();
                        p.serializedObject.Update();
                    },
                    sort: nodes => nodes
                        .OrderBy(n => n.data.data.library == null)
                        .ThenBy(n => n.data.data.array)
                        .ThenBy(n => n.data.data.usage == LabelArrayUsage.ByIndex ? n.data.data.index : 0)
                        .ThenBy(n => n.data.data.usage == LabelArrayUsage.ByHash ? n.data.name : null),
                    pathToName: pathParts => string.Join(Separator, pathParts)
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
            
            var fieldType = propertyFieldInfo.FieldType;
            var elementType = fieldType.IsArray ? fieldType.GetElementType() ?? fieldType : fieldType;
            var genericType = elementType.IsGenericType ? elementType.GetGenericArguments()[0] : null;
            
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
                var usage = library.GetArrayUsage(i);
                
                if (none) {
                    path = GetEntryPath(libName, arrayName, None);
                    if (!IsValidPath(path, filters)) continue;
                    
                    list.Add(new Entry(library, library.GetArrayId(i), index: 0, array: i, path, usage));
                }

                int labelsCount = library.GetLabelsCount(i);
                
                for (int j = 0; j < labelsCount; j++) {
                    int id = library.GetLabelId(i, j);
                    path = GetEntryPath(libName, arrayName, library.GetLabel(id));
                    
                    if (!IsValidPath(path, filters)) continue;
                    
                    list.Add(new Entry(library, library.GetLabelId(i, j), index: none ? j + 1 : j, array: i, path, usage));
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
                if (path.IsSubPathOf(filterPath)) return true;
            }

            return false;
        }

        private static string GetEntryPath(string library, string array, string value) {
            return string.IsNullOrWhiteSpace(array)
                    ? $"{library}/{value}"
                    : $"{library}/{array}/{value}";
        }

        private static GUIContent GetDropdownLabel(LabelLibraryBase library, int id) {
            if (library == null) return NullLabel;

            if (!library.ContainsLabel(id)) {
                return new GUIContent($"{library.name}{Separator}Id [{id}] {NotFound}");
            }

            int arrayCount = library.GetArraysCount();
            int array = library.GetArrayIndex(id);
            int arrayId = library.GetArrayId(array);
            
            string arrayName = library.GetArrayName(array);
            arrayName = string.IsNullOrWhiteSpace(arrayName) 
                ? arrayCount == 1 
                    ? Separator 
                    : $"{Separator}Array [{array}]{Separator}" 
                : $"{Separator}{arrayName}{Separator}";

            bool none = library.GetArrayNoneLabel(array);
            if (none && id == arrayId) {
                return new GUIContent($"{library.name}{arrayName}{None}");
            }

            string label = library.GetLabel(id);
            
            return string.IsNullOrWhiteSpace(label)
                ? new GUIContent($"{library.name}{arrayName}Label [{library.GetLabelIndex(id)}]")
                : new GUIContent($"{library.name}{arrayName}{label}");
        }
    }
    
}