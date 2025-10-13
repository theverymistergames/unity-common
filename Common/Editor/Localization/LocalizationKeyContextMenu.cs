using MisterGames.Common.Data;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Common.Localization;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Localization {
    
    [InitializeOnLoad]
    internal static class LocalizationKeyContextMenu {
        
        static LocalizationKeyContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Generic ||
                property.FindPropertyRelative(nameof(LocalizationKey.hash)) is not { } hashProperty ||
                property.FindPropertyRelative(nameof(LocalizationKey.table)) is not {} tableProperty) 
            {
                return;
            }
            
            SerializedPropertyExtensions.ReadSerializedGuid(tableProperty, out ulong low, out ulong high);
            
            var key = new LocalizationKey(hashProperty.intValue, HashHelpers.ComposeGuid(low, high));
            var table = LocalizationKeyExtensions.LoadTableStorageAssetInEditor(key, out int keyIndex);
            
            if (table == null) return;
            
            menu.AddItem(new GUIContent("Select Localization Table"), false, () => {
                EditorGUIUtility.PingObject(table);
            });
        }
    }
    
}