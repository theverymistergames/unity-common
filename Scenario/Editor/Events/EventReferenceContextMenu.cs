using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {
    
    [InitializeOnLoad]
    public static class EventReferenceContextMenu {

        static EventReferenceContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.Generic ||
                property.FindPropertyRelative("_eventDomain") is not { objectReferenceValue: EventDomain eventDomain } ||
                property.FindPropertyRelative("_eventId") is null ||
                property.FindPropertyRelative("_subId") is null
            ) {
                return;
            }
            
            menu.AddItem(new GUIContent("Select Event Domain"), false, () => {
                EditorGUIUtility.PingObject(eventDomain);
            });
        }
    }
    
}