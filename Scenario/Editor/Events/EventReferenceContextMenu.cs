using System.Collections.Generic;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {
    
    [InitializeOnLoad]
    internal static class EventReferenceContextMenu {
        
        private const string LibraryPropertyPath = nameof(EventReference._eventDomain);
        private const string IdPropertyPath = nameof(EventReference._eventId);
        private const string EntryNamePropertyPath = nameof(EventDomain.EventEntry.name);
        private const string EntryIdPropertyPath = nameof(EventDomain.EventEntry.id);
        private const string GroupNamePropertyPath = nameof(EventDomain.EventGroup.name);
        
        static EventReferenceContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            CheckEventReference(menu, property);
            CheckEventEntry(menu, property);
            CheckEventGroup(menu, property);
            CheckEventArray(menu, property);
        }

        private static void CheckEventReference(GenericMenu menu, SerializedProperty property) {
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
        
        private static void CheckEventEntry(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.String ||
                property.name != EntryNamePropertyPath ||
                property.serializedObject.targetObject is not EventDomain eventDomain) {
                return;
            }
            
            var parentProp = property.GetParentProperty();
            
            if (parentProp?.type != nameof(EventDomain.EventEntry) ||
                parentProp.FindPropertyRelative(EntryIdPropertyPath) is not { propertyType: SerializedPropertyType.Integer } idProperty) {
                return;
            }

            if (idProperty.intValue != 0) {
                menu.AddItem(new GUIContent("Search usages..."), false, () => {
                    EventReferenceSearchWindow.SearchEventReference(new EventReference(eventDomain, idProperty.intValue));
                });
            }
        }
        
        private static void CheckEventGroup(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.String ||
                property.name != GroupNamePropertyPath ||
                property.serializedObject.targetObject is not EventDomain eventDomain) {
                return;
            }

            var parentProp = property.GetParentProperty();
            if (parentProp?.type != nameof(EventDomain.EventGroup)) return;

            int groupIndex = ParseArrayIndex(parentProp.propertyPath);

            menu.AddItem(new GUIContent("Search usages..."), false, () => {
                var groups = eventDomain.EventGroups;
                if (groupIndex >= groups.Length) return;

                var events = groups[groupIndex].events;
                if (events == null) return;

                var refs = new List<EventReference>(events.Length);
                for (int i = 0; i < events.Length; i++) {
                    ref var e = ref events[i];
                    if (e.id != 0) refs.Add(new EventReference(eventDomain, e.id));
                }

                EventReferenceSearchWindow.SearchEventReferences(refs.ToArray());
            });
        }

        private static void CheckEventArray(GenericMenu menu, SerializedProperty property) {
            if (!property.isArray ||
                property.arrayElementType != nameof(EventDomain.EventEntry) ||
                property.serializedObject.targetObject is not EventDomain eventDomain) {
                return;
            }

            var parentProp = property.GetParentProperty();
            if (parentProp?.type != nameof(EventDomain.EventGroup)) return;

            int groupIndex = ParseArrayIndex(parentProp.propertyPath);

            menu.AddItem(new GUIContent("Search usages..."), false, () => {
                var groups = eventDomain.EventGroups;
                if (groupIndex >= groups.Length) return;

                var events = groups[groupIndex].events;
                if (events == null) return;

                var refs = new List<EventReference>(events.Length);
                for (int i = 0; i < events.Length; i++) {
                    ref var e = ref events[i];
                    if (e.id != 0) refs.Add(new EventReference(eventDomain, e.id));
                }

                EventReferenceSearchWindow.SearchEventReferences(refs.ToArray());
            });
        }

        private static int ParseArrayIndex(string path) {
            int start = path.LastIndexOf('[');
            int end = path.LastIndexOf(']');
            return start >= 0 && end > start && int.TryParse(path.Substring(start + 1, end - start - 1), out int index)
                ? index
                : -1;
        }
    }
    
}