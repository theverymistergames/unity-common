using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {
    
    [InitializeOnLoad]
    internal static class EventReferenceContextMenu {
        
        private const string LibraryPropertyPath = nameof(EventReference._eventDomain);
        private const string IdPropertyPath = nameof(EventReference._eventId);
        
        static EventReferenceContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Generic ||
                property.FindPropertyRelative(LibraryPropertyPath) is not { objectReferenceValue: EventDomain eventDomain } ||
                property.FindPropertyRelative(IdPropertyPath) is not { } idProperty
            ) {
                return;
            }
            
            menu.AddItem(new GUIContent("Select EventDomain"), false, () => {
                EditorGUIUtility.PingObject(eventDomain);
            });
            
            if (idProperty.intValue != 0) {
                menu.AddItem(new GUIContent("Search usages..."), false, () => {
                    EventReferenceSearchWindow.SearchEventReference(new EventReference(eventDomain, idProperty.intValue));
                });
            }
        }
    }
    
}