using MisterGames.Common.Labels;
using MisterGames.Common.Labels.Base;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Labels {
    
    [InitializeOnLoad]
    internal static class LabelValueContextMenu {
        
        private const string LibraryPropertyPath = nameof(LabelValue.library);
        private const string IdPropertyPath = nameof(LabelValue.id);
        
        static LabelValueContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            CheckLabelValue(menu, property);
        }

        private static void CheckLabelValue(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Generic ||
                property.FindPropertyRelative(LibraryPropertyPath) is not { objectReferenceValue: LabelLibraryBase labelLibrary } ||
                property.FindPropertyRelative(IdPropertyPath) is not { } idProperty
               ) {
                return;
            }
            
            menu.AddItem(new GUIContent("Select LabelLibrary"), false, () => {
                EditorGUIUtility.PingObject(labelLibrary);
            });
            
            if (idProperty.intValue != 0) {
                menu.AddItem(new GUIContent("Search usages..."), false, () => {
                    LabelValueSearchWindow.SearchLabelValue(new LabelValue(labelLibrary, idProperty.intValue));
                });
            }
        }
    }
    
}