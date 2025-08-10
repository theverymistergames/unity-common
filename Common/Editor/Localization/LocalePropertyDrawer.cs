using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Localization;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Localization {
    
    [CustomPropertyDrawer(typeof(Locale))]
    public sealed class LocalePropertyDrawer : PropertyDrawer {
        
        private const string Null = "<null>";
        
        private static readonly GUIContent NullLabel = new(Null);

        private const string LocaleHashPath = nameof(Locale.hash);
        private const string LocalizationSettingsPath = nameof(Locale.localizationSettings);

        private readonly struct Entry : IEquatable<Entry> {
            
            public readonly LocalizationSettings localizationSettings;
            public readonly LocaleDescriptor descriptor;
            
            public Entry(LocalizationSettings localizationSettings, LocaleDescriptor descriptor) {
                this.localizationSettings = localizationSettings;
                this.descriptor = descriptor;
            }

            public bool Equals(Entry other) => Equals(localizationSettings, other.localizationSettings) && descriptor.Equals(other.descriptor);
            public override bool Equals(object obj) => obj is Entry other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(localizationSettings, descriptor);
            public static bool operator ==(Entry left, Entry right) => left.Equals(right);
            public static bool operator !=(Entry left, Entry right) => !left.Equals(right);
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
            
            var hashProperty = property.FindPropertyRelative(LocaleHashPath);
            var localizationSettingsProperty = property.FindPropertyRelative(LocalizationSettingsPath);
            
            int hash = hashProperty.intValue;
            var localizationSettings = localizationSettingsProperty.objectReferenceValue as LocalizationSettings;
            var root = property.serializedObject.targetObject as LocalizationSettings;
            
            if (EditorGUI.DropdownButton(rect, GetDropdownLabel(root, localizationSettings, hash), FocusType.Keyboard)) {
                
                var dropdown = new AdvancedDropdown<Entry>(
                    "Select locale",
                    GetAllEntries(root, GetFilter(fieldInfo)),
                    e => GetPath(root, e),
                    onItemSelected: (e, _) => {
                        var p = property.Copy();
                        
                        property.FindPropertyRelative(LocalizationSettingsPath).objectReferenceValue = e.localizationSettings; 
                        property.FindPropertyRelative(LocaleHashPath).intValue = string.IsNullOrWhiteSpace(e.descriptor.code) 
                            ? 0 
                            : Animator.StringToHash(e.descriptor.code);
                        
                        p.serializedObject.ApplyModifiedProperties();
                        p.serializedObject.Update();
                    },
                    sort: nodes => nodes.OrderBy(n => n.data.data.descriptor.code)
                );
                
                dropdown.Show(rect);
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        private static string GetPath(LocalizationSettings root, Entry entry) {
            return root != null || entry.localizationSettings == null
                ? GetLocaleLabel(entry.descriptor)
                : $"{entry.localizationSettings.name} : {GetLocaleLabel(entry.descriptor)}";
        }

        private static string GetLocaleLabel(LocaleDescriptor localeDescriptor) {
            return string.IsNullOrWhiteSpace(localeDescriptor.code)
                ? Null
                : $"{localeDescriptor.code} ({localeDescriptor.description})";
        }

        private static LocaleFilter GetFilter(FieldInfo propertyFieldInfo) {
            var filters = propertyFieldInfo.GetCustomAttributes<LocaleFilterAttribute>().ToArray();
            var filter = LocaleFilter.Supported;
            
            for (int i = 0; i < filters.Length; i++) {
                var f = filters[i].filter;
                
                if (i == 0) {
                    filter = f;
                    continue;
                }

                if (f == LocaleFilter.All || filter == LocaleFilter.All || filter != f) {
                    return LocaleFilter.All;
                }
            }
            
            return filter;
        }

        private static IEnumerable<Entry> GetAllEntries(LocalizationSettings root, LocaleFilter filter) {
            return filter switch {
                LocaleFilter.Supported => GetSupportedLocales(root).Prepend(default),
                LocaleFilter.Hardcoded => GetHardcodedLocales().Prepend(default),
                LocaleFilter.All => GetHardcodedLocales().Concat(GetSupportedLocales(root)).Distinct().Prepend(default),
                _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
            };
        }

        private static IEnumerable<Entry> GetHardcodedLocales() {
            return LocaleExtensions.LocaleIds
                .Select(id => LocaleExtensions.TryGetLocaleDescriptorById(id, out var desc) ? new Entry(null, desc) : default);
        }

        private static IEnumerable<Entry> GetSupportedLocales(LocalizationSettings root) {
            if (root != null) {
                return root.GetSupportedLocales().Select(l => new Entry(l.localizationSettings, l.GetDescriptor()));
            }
            
            return AssetDatabase
                .FindAssets($"a:assets t:{nameof(LocalizationSettings)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<LocalizationSettings>(AssetDatabase.GUIDToAssetPath(guid)))
                .SelectMany(s => s.GetSupportedLocales())
                .Distinct()
                .Select(l => new Entry(l.localizationSettings, l.GetDescriptor()));
        }

        private static GUIContent GetDropdownLabel(LocalizationSettings root, LocalizationSettings localizationSettings, int hash) {
            LocaleDescriptor descriptor;
            
            if (localizationSettings == null) {
                if (LocaleExtensions.TryGetLocaleDescriptorByHash(hash, out descriptor)) {
                    return new GUIContent(GetLocaleLabel(descriptor));
                }

                return NullLabel;
            }

            if (localizationSettings.TryGetLocaleDescriptorByHash(hash, out descriptor)) {
                string label = root != null
                    ? GetLocaleLabel(descriptor)
                    : $"{localizationSettings.name} : {GetLocaleLabel(descriptor)}";
                
                return new GUIContent(label);
            }

            return NullLabel;
        }
    }
    
}