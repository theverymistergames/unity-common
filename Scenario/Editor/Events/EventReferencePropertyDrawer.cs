using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Editor.Views;
using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {

    [CustomPropertyDrawer(typeof(EventReference))]
    public class EventReferencePropertyDrawer : PropertyDrawer {

        private const string NULL = "null";
        private static readonly GUIContent NULL_LABEL = new GUIContent(NULL);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var eventDomainProperty = property.FindPropertyRelative("_eventDomain");
            var eventIdProperty = property.FindPropertyRelative("_eventId");
            var eventDomain = eventDomainProperty.objectReferenceValue as EventDomain;
            GUIContent eventLabel;

            if (eventDomain == null || eventDomain.GetEventPath(eventIdProperty.intValue) is not {} eventPath) {
                eventIdProperty.intValue = 0;

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();

                eventLabel = NULL_LABEL;
            }
            else {
                eventLabel = new GUIContent(eventPath);
            }

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, eventDomainProperty, label);

            rect = new Rect(
                position.x,
                position.y + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight,
                position.width,
                EditorGUIUtility.singleLineHeight
            );

            if (EditorGUI.DropdownButton(rect, eventLabel, FocusType.Keyboard)) {
                CreateDropdown(property).Show(position);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
        }

        private static AdvancedDropdown<EventReference> CreateDropdown(SerializedProperty property) {
            var eventReferences = new List<EventReference> { new EventReference(null, 0) };

            if (property.FindPropertyRelative("_eventDomain").objectReferenceValue is EventDomain eventDomain) {
                var eventGroups = eventDomain.EventGroups;

                for (int j = 0; j < eventGroups.Length; j++) {
                    ref var eventGroup = ref eventGroups[j];
                    var events = eventGroup.events;

                    for (int k = 0; k < events.Length; k++) {
                        ref var entry = ref events[k];
                        if (string.IsNullOrWhiteSpace(entry.name)) continue;

                        eventReferences.Add(new EventReference(eventDomain, entry.id));
                    }
                }
            }

            return new AdvancedDropdown<EventReference>(
                "Select event",
                eventReferences,
                e => e.EventDomain == null ? NULL : e.EventDomain.GetEventPath(e.EventId),
                e => {
                    property = property.Copy();

                    property.FindPropertyRelative("_eventDomain").objectReferenceValue = e.EventDomain;
                    property.FindPropertyRelative("_eventId").intValue = e.EventId;

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                },
                separator: '/',
                sort: children => children
                    .OrderByDescending(n => n.data.data.EventDomain == null && n.children.Count == 0)
                    .ThenBy(n => n.children.Count == 0)
            );
        }
    }

}
