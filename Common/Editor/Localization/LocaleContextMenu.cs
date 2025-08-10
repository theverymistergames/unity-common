using MisterGames.Common.Localization;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Localization {
    
    [InitializeOnLoad]
    internal static class LocaleContextMenu {
        
        private const string LocaleHashPath = nameof(Locale.hash);
        private const string LocalizationSettingsPath = nameof(Locale.localizationSettings);
        
        static LocaleContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Generic ||
                property.FindPropertyRelative(LocalizationSettingsPath) is not { objectReferenceValue: LocalizationSettings localizationSettings } ||
                property.FindPropertyRelative(LocaleHashPath) is null
            ) {
                return;
            }
            
            menu.AddItem(new GUIContent("Select Localization Settings"), false, () => {
                EditorGUIUtility.PingObject(localizationSettings);
            });
        }
    }
    
}