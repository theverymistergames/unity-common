using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Editor.Views;
using MisterGames.Common.Maths;
using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {

    [CustomPropertyDrawer(typeof(EventReference))]
    public sealed class EventReferencePropertyDrawer : PropertyDrawer {
        
        private const string Separator = " : ";
        private const string Null = "<null>";
        private const string NotFound = "(not found)";

        private const string EventDomainPropertyPath = "_eventDomain";
        private const string EventIdPropertyPath = "_eventId";
        private const string SubIdPropertyPath = "_subId";

        private const float SubIdWidthRatio = 0.12f;

        private static readonly GUIContent NullLabel = new(Null);
        private static readonly Color BoxColor = new Color(0.2f, 0.2f, 0.2f);

        private readonly struct Entry {

            public readonly EventDomain eventDomain;
            public readonly int eventId;
            public readonly string name;
            public readonly string group;
            public readonly int index;

            public Entry(EventDomain eventDomain, int eventId, string name, string group, int index) {
                this.eventDomain = eventDomain;
                this.eventId = eventId;
                this.name = name;
                this.group = group;
                this.index = index;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            
            bool hasLabel = label != null && label != GUIContent.none;
            float offset = hasLabel.AsFloat() * (EditorGUIUtility.labelWidth + 2f);

            var rect = position;
            rect.x += offset;
            rect.width -= offset;

            rect.height += 2f * EditorGUIUtility.standardVerticalSpacing;
            rect.y -= EditorGUIUtility.standardVerticalSpacing;
            
            var oldColor = GUI.color;
            GUI.color = BoxColor;
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
            GUI.color = oldColor;

            rect = position;
            float indent = EditorGUI.indentLevel * 15;
            rect.x += indent + EditorGUIUtility.standardVerticalSpacing;
            rect.width -= indent + 2f * EditorGUIUtility.standardVerticalSpacing;
            rect.height = EditorGUIUtility.singleLineHeight;
            
            var eventDomainProperty = property.FindPropertyRelative(EventDomainPropertyPath);
            var eventIdProperty = property.FindPropertyRelative(EventIdPropertyPath);
            var subIdProperty = property.FindPropertyRelative(SubIdPropertyPath);

            EditorGUI.PropertyField(rect, eventDomainProperty, label);
            
            var eventDomain = eventDomainProperty.objectReferenceValue as EventDomain;
            int eventId = eventIdProperty.intValue;

            if (eventDomain == null) {
                eventId = 0;
                eventIdProperty.intValue = -1;
                subIdProperty.intValue = 0;

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }
            
            float subIdWidth = rect.width * SubIdWidthRatio;
            
            rect = position;
            rect.x += offset + EditorGUIUtility.standardVerticalSpacing;
            rect.width -= offset + subIdWidth + EditorGUIUtility.standardVerticalSpacing;
            
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            rect.height = EditorGUIUtility.singleLineHeight;
            
            if (EditorGUI.DropdownButton(rect, GetDropdownLabel(eventDomain, eventId), FocusType.Keyboard)) {
                bool hasCurrentDomain = eventDomain != null;
                
                var dropdown = new AdvancedDropdown<Entry>(
                    "Select event",
                    hasCurrentDomain ? GetDomainEntries(eventDomain) : GetAllEntries(),
                    e => GetEntryPath(e, includeDomain: !hasCurrentDomain),
                    e => {
                        var p = property.Copy();

                        property.FindPropertyRelative(EventDomainPropertyPath).objectReferenceValue = e.eventDomain;
                        property.FindPropertyRelative(EventIdPropertyPath).intValue = e.eventId;
                        property.FindPropertyRelative(SubIdPropertyPath).intValue = 0;

                        p.serializedObject.ApplyModifiedProperties();
                        p.serializedObject.Update();
                    },
                    sort: nodes => nodes
                        .OrderBy(n => n.data.data.eventDomain == null)
                        .ThenBy(n => n.data.data.index)
                        .ThenBy(n => n.data.name)
                );

                dropdown.Show(rect);
            }

            rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing;
            rect.width = subIdWidth - 2f * EditorGUIUtility.standardVerticalSpacing;
            
            subIdProperty.intValue = EditorGUI.IntField(rect, GUIContent.none, subIdProperty.intValue);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
        }

        private static IEnumerable<Entry> GetAllEntries() {
            return AssetDatabase
                .FindAssets($"a:assets t:{nameof(EventDomain)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<EventDomain>(AssetDatabase.GUIDToAssetPath(guid)))
                .SelectMany(GetDomainEntries);
        }

        private static IEnumerable<Entry> GetDomainEntries(EventDomain eventDomain) {
            if (eventDomain == null || eventDomain.EventGroups.Length <= 0) return Array.Empty<Entry>();

            var list = new List<Entry>();
            var eventGroups = eventDomain.EventGroups;
            
            for (int i = 0; i < eventGroups.Length; i++) {
                var group = eventGroups[i];
                
                string groupName = string.IsNullOrWhiteSpace(group.name)
                    ? eventGroups.Length == 1 ? null : $"Group [{i}]"
                    : group.name;
                
                for (int j = 0; j < group.events?.Length; j++) {
                    var e = group.events[j];
                    list.Add(new Entry(eventDomain, e.id, e.name, groupName, j));
                }
            }

            return list;
        }

        private static string GetEntryPath(Entry entry, bool includeDomain) {
            return entry.eventDomain == null
                ? Null
                : string.IsNullOrWhiteSpace(entry.group)
                    ? $"{(includeDomain ? $"{entry.eventDomain.name}/" : string.Empty)}{entry.name}"
                    : $"{(includeDomain ? $"{entry.eventDomain.name}/" : string.Empty)}{entry.group}/{entry.name}";
        }

        private static GUIContent GetDropdownLabel(EventDomain eventDomain, int eventId) {
            if (eventDomain == null) return NullLabel;
            
            if (!eventDomain.TryGetAddress(eventId, out int group, out int index)) {
                return new GUIContent($"Event [{eventId}] {NotFound}");
            }

            var eventGroups = eventDomain.EventGroups;
            ref var eventGroup = ref eventGroups[group];
            
            string groupName = string.IsNullOrWhiteSpace(eventGroup.name)
                ? eventDomain.EventGroups.Length == 1
                    ? string.Empty
                    : $"Group [{group}]{Separator}"
                : $"{eventGroup.name}{Separator}";

            string eventName = eventDomain.GetEventName(eventId);
            eventName = string.IsNullOrWhiteSpace(eventName) ? $"Event [{index}]" : eventName;
            
            return new GUIContent($"{groupName}{eventName}");
        }
    }

}
