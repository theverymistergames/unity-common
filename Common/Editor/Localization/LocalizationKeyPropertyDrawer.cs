using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Localization;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Localization {
    
    [CustomPropertyDrawer(typeof(LocalizationKey))]
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
            float indent = EditorGUI.indentLevel * 15;
            float offset = indent - 1f;
            rect.x += offset;
            rect.width -= offset;
            
            GUI.Label(rect, label);

            rect = position;
            offset = EditorGUIUtility.labelWidth + 2f;
            rect.x += offset;
            rect.width -= offset;
            
            var hashProperty = property.FindPropertyRelative(KeyHashPath);
            var tableGuidProperty = property.FindPropertyRelative(TableGuidPath);
            
            int hash = hashProperty.intValue;
            string tableGuid = tableGuidProperty.stringValue;
            
            if (EditorGUI.DropdownButton(rect, GetDropdownLabel(hash, tableGuid), FocusType.Keyboard)) {
                var dropdown = new AdvancedDropdown<Entry>(
                    "Select localization key",
                    GetAllEntries(),
                    GetPath,
                    onItemSelected: (e, _) => {
                        var p = property.Copy();
                        
                        property.FindPropertyRelative(TableGuidPath).stringValue = e.table == null
                            ? null
                            : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(e.table));
                        
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
            return EditorGUIUtility.singleLineHeight;
        }

        private static string GetPath(Entry entry) {
            return entry.table != null ? $"{entry.table.name}/{entry.key}" : Null;
        }

        private static IEnumerable<Entry> GetAllEntries() {
            return AssetDatabase
                .FindAssets($"a:assets t:{nameof(LocalizationTableStorageBase)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<LocalizationTableStorageBase>(AssetDatabase.GUIDToAssetPath(guid)))
                .SelectMany(GetKeys)
                .Prepend(default);
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

        private static GUIContent GetDropdownLabel(int hash, string tableGuid) {
            if (string.IsNullOrEmpty(tableGuid) ||
                AssetDatabase.LoadAssetAtPath<LocalizationTableStorageBase>(AssetDatabase.GUIDToAssetPath(tableGuid)) is not { } table) 
            {
                return NullLabel;
            }
            
            if (!table.TryGetKey(hash, out string key)) {
                return new GUIContent($"{NotFound} {table.name} : hash {hash}");
            }

            return new GUIContent($"{table.name} : {key}");
        }
    }
    
}