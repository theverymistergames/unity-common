using MisterGames.Input.Actions;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Editor.Menu {
    
    [InitializeOnLoad]
    internal static class InputRefsContextMenu {
        
        static InputRefsContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Generic ||
                property.FindPropertyRelative(nameof(InputActionRef._guid)) is null)
            {
                return;
            }
            
            menu.AddItem(new GUIContent("Select default InputActionsAsset"), false, () => {
                EditorGUIUtility.PingObject(InputSystem.actions);
            });
        }
    }
    
}