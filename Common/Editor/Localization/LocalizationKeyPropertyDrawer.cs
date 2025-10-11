using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Localization;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Localization {
    
    [CustomPropertyDrawer(typeof(LocalizationKey))]
    [CustomPropertyDrawer(typeof(LocalizationKey<>))]
    public sealed class LocalizationKeyPropertyDrawer : PropertyDrawer {
        
        private const string Null = "<null>";
        private const string NotFound = "(not found)";
        
        private static readonly GUIContent NullLabel = new(Null);

        private const string KeyHashPath = nameof(LocalizationKey.hash);
        private const string TableGuidPath = nameof(LocalizationKey.tableGuid);

        private readonly struct Entry {
            
            public readonly LocalizationTableStorageBase table;
            public readonly string key;
            
            public Entry(LocalizationTableStorageBase table, string key) {
                this.table = table;
                this.key = key;
            }
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            
            var rect = position;
            float offset = EditorGUIUtility.labelWidth + 2f;
            rect.x += offset;
            rect.width -= offset;
            rect.height = EditorGUIUtility.singleLineHeight;
            
            var hashProperty = property.FindPropertyRelative(KeyHashPath);
            var tableGuidProperty = property.FindPropertyRelative(TableGuidPath);
            
            int hash = hashProperty.intValue;
            string tableGuid = GetUnityEditorGuid(tableGuidProperty);
            
            var table = string.IsNullOrWhiteSpace(tableGuid) 
                ? null 
                : AssetDatabase.LoadAssetAtPath<LocalizationTableStorageBase>(AssetDatabase.GUIDToAssetPath(tableGuid));
            
            var foldoutRect = position;
            foldoutRect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, toggleOnLabelClick: true);
            
            if (property.isExpanded && table != null) {
                var serializedObject = new SerializedObject(table);
                var valuesProperty = serializedObject.FindProperty(table.GetValuesPropertyPath(hash));

                var valuesRect = position;
                valuesRect.height = EditorGUI.GetPropertyHeight(valuesProperty, includeChildren: true);
                    
                if (valuesProperty != null) {
                    EditorGUI.BeginDisabledGroup(true);

                    var valueRect = valuesRect;
                    valueRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        
                    for (int i = 0; i < valuesProperty.arraySize; i++) {
                        var valueProperty = valuesProperty.GetArrayElementAtIndex(i);
                            
                        valueRect.height = EditorGUI.GetPropertyHeight(valueProperty, includeChildren: true);
                            
                        EditorGUI.PropertyField(valueRect, valueProperty, includeChildren: true);
                            
                        valueRect.y += valueRect.height + EditorGUIUtility.standardVerticalSpacing;
                    }
                        
                    EditorGUI.EndDisabledGroup();
                }
            }
            
            if (EditorGUI.DropdownButton(rect, GetDropdownLabel(hash, table, fieldInfo), FocusType.Keyboard)) {
                var dropdown = new AdvancedDropdown<Entry>(
                    "Select localization key",
                    GetAllEntries(fieldInfo),
                    GetPath,
                    onItemSelected: (e, _) => {
                        var p = property.Copy();

                        ulong low;
                        ulong high;
                        
                        if (e.table == null) {
                            low = 0;
                            high = 0;
                        }
                        else {
                            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(e.table));
                            (low, high) = new Guid(guid).DecomposeGuid();
                        }
                        
                        property.FindPropertyRelative(TableGuidPath).FindPropertyRelative("_guidLow").ulongValue = low;
                        property.FindPropertyRelative(TableGuidPath).FindPropertyRelative("_guidHigh").ulongValue = high;
                        
                        property.FindPropertyRelative(KeyHashPath).intValue = string.IsNullOrWhiteSpace(e.key)
                            ? 0
                            : Animator.StringToHash(e.key);
                        
                        p.serializedObject.ApplyModifiedProperties();
                        p.serializedObject.Update();
                    },
                    sort: nodes => nodes.OrderBy(n => n.data.data.key),
                    pathToName: parts => string.Join(" : ", parts)
                );
                
                dropdown.Show(rect);
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (property.isExpanded) {
                string tableGuid = GetUnityEditorGuid(property.FindPropertyRelative(TableGuidPath));
                var table = string.IsNullOrWhiteSpace(tableGuid) 
                    ? null 
                    : AssetDatabase.LoadAssetAtPath<LocalizationTableStorageBase>(AssetDatabase.GUIDToAssetPath(tableGuid));

                if (table != null) {
                    int hash = property.FindPropertyRelative(KeyHashPath).intValue;
                
                    var serializedObject = new SerializedObject(table);
                    var valuesProperty = serializedObject.FindProperty(table.GetValuesPropertyPath(hash));

                    if (valuesProperty != null) {
                        return EditorGUI.GetPropertyHeight(valuesProperty, includeChildren: true) + 
                               EditorGUIUtility.standardVerticalSpacing * 3f - 
                               EditorGUIUtility.singleLineHeight * 2f;
                    }
                }
            }

            return EditorGUIUtility.singleLineHeight;
        }

        private static string GetUnityEditorGuid(SerializedProperty guidProperty) {
            ulong low = guidProperty.FindPropertyRelative("_guidLow").ulongValue;
            ulong high = guidProperty.FindPropertyRelative("_guidHigh").ulongValue;

            return HashHelpers.ComposeGuid(low, high).FormatUnityEditorGUID();
        }

        private static string GetPath(Entry entry) {
            return entry.table != null ? $"{entry.table.name}/{entry.key}" : Null;
        }

        private static IEnumerable<Entry> GetAllEntries(FieldInfo fieldInfo) {
            var fieldType = fieldInfo.FieldType;
            var elementType = fieldType.IsArray ? fieldType.GetElementType() ?? fieldType : fieldType;
            var genericType = elementType.IsGenericType ? elementType.GetGenericArguments()[0] : null;
            
            return AssetDatabase
                .FindAssets($"a:assets t:{nameof(LocalizationTableStorageBase)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<LocalizationTableStorageBase>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(t => IsValidTableType(t, genericType))
                .SelectMany(GetKeys)
                .Prepend(default);
        }
        
        private static bool IsValidTableType(LocalizationTableStorageBase table, Type genericType) {
            var dataType = table.GetDataType();
            return genericType?.IsAssignableFrom(dataType) ?? dataType == null;
        }

        private static IEnumerable<Entry> GetKeys(LocalizationTableStorageBase table) {
            int count = table.GetKeyCount();
            if (count <= 0) return Array.Empty<Entry>();
            
            var keys = new List<Entry>(count);
            for (int i = 0; i < count; i++) {
                string key = table.GetKey(i);
                if (string.IsNullOrWhiteSpace(key)) continue;
                
                keys.Add(new Entry(table, key));
            }
            
            return keys;
        }

        private static GUIContent GetDropdownLabel(int hash, LocalizationTableStorageBase table, FieldInfo propertyFieldInfo) {
            if (table == null) {
                return NullLabel;
            }
            
            if (!table.TryGetKey(hash, out string key)) {
                return new GUIContent($"{NotFound} {table.name} : hash {hash}");
            }

            bool hideTable = propertyFieldInfo.GetCustomAttribute<HideLocalizationTableAttribute>() != null;
            
            return hideTable
                ? new GUIContent(key)
                : new GUIContent($"{table.name} : {key}");
        }
    }
    
}