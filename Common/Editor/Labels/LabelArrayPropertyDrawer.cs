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
    
    [CustomPropertyDrawer(typeof(LabelArray))]
    [CustomPropertyDrawer(typeof(LabelArray<>))]
    public sealed class LabelArrayPropertyDrawer : PropertyDrawer {
        
        private const string Separator = " : ";
        private const string Null = "<null>";
        private const string NotFound = "(not found)";

        private const string LibraryPropertyPath = nameof(LabelArray.library);
        private const string IdPropertyPath = nameof(LabelArray.id);

        private static readonly GUIContent NullLabel = new(Null);
        
        private readonly struct Entry {
            
            public readonly LabelLibraryBase library;
            public readonly int arrayIndex;
            public readonly int arrayId;
            public readonly string path;

            public Entry(LabelLibraryBase library, int arrayId, int arrayIndex, string path) {
                this.library = library;
                this.arrayId = arrayId;
                this.arrayIndex = arrayIndex;
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

            rect = position;
            offset = EditorGUIUtility.labelWidth + 2f;
            rect.x += offset;
            rect.width -= offset;
            
            var libraryProperty = property.FindPropertyRelative(LibraryPropertyPath); 
            var idProperty = property.FindPropertyRelative(IdPropertyPath);

            if (libraryProperty == null || idProperty == null) {
                GUI.Label(rect, $"Error: {property.propertyPath} is not a LabelArray");
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
                        property.FindPropertyRelative(IdPropertyPath).intValue = e.arrayId; 
                        
                        p.serializedObject.ApplyModifiedProperties();
                        p.serializedObject.Update();
                    },
                    sort: nodes => nodes
                        .OrderBy(n => n.data.data.library == null)
                        .ThenBy(n => n.data.data.arrayIndex),
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
                
                string path = string.IsNullOrWhiteSpace(arrayName) 
                    ? $"{libName}/Array [{i}]"
                    : $"{libName}/{arrayName}";
                
                if (!IsValidPath(path, filters)) continue;
                
                list.Add(new Entry(library, library.GetArrayId(i), arrayIndex: i, path));
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

        private static GUIContent GetDropdownLabel(LabelLibraryBase library, int id) {
            if (library == null) return NullLabel;

            int arrayIndex = library.GetArrayIndex(id);
            
            if (arrayIndex < 0) {
                return new GUIContent($"{library.name}{Separator}Id [{id}] {NotFound}");
            }

            string arrayName = library.GetArrayName(arrayIndex);
            return string.IsNullOrWhiteSpace(arrayName)
                ? new GUIContent($"{library.name}{Separator}Array [{arrayIndex}]")
                : new GUIContent($"{library.name}{Separator}{arrayName}");
        }
    }
    
}