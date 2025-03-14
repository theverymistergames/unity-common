﻿using MisterGames.Common.Labels.Base;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Labels {
    
    [InitializeOnLoad]
    public static class LabelValueContextMenu {
        
        static LabelValueContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Generic ||
                property.FindPropertyRelative("library") is not { objectReferenceValue: LabelLibraryBase labelLibrary } ||
                property.FindPropertyRelative("id") is null
            ) {
                return;
            }
            
            menu.AddItem(new GUIContent("Select Label Library"), false, () => {
                EditorGUIUtility.PingObject(labelLibrary);
            });
        }
    }
    
}