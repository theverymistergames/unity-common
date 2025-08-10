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
            if (property.propertyType != SerializedPropertyType.Generic ||
                property.FindPropertyRelative(LibraryPropertyPath) is not { objectReferenceValue: LabelLibraryBase labelLibrary } ||
                property.FindPropertyRelative(IdPropertyPath) is null
            ) {
                return;
            }
            
            menu.AddItem(new GUIContent("Select Label Library"), false, () => {
                EditorGUIUtility.PingObject(labelLibrary);
            });
        }
    }
    
}