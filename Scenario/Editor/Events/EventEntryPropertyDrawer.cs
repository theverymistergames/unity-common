using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {

    [CustomPropertyDrawer(typeof(EventDomain.EventEntry))]
    public class EventEntryPropertyDrawer : PropertyDrawer {

        private static readonly GUIContent RaisedEventsLabel = new("Raised events");

        private const float DividerDefault = 3f;
        private const float DividerGroup = 60f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var eventDomain = property.serializedObject.targetObject as EventDomain;
            var eventNameProp = property.FindPropertyRelative(nameof(EventDomain.EventEntry.name));
            var eventIdProp = property.FindPropertyRelative(nameof(EventDomain.EventEntry.id));
            var subIdProperty = property.FindPropertyRelative(nameof(EventDomain.EventEntry.subId));
            
            float subIdWidth = position.width * 0.1f;
            
            var rect = position;
            
            rect.width -= EditorGUIUtility.singleLineHeight + DividerDefault + DividerGroup + subIdWidth;
            rect.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.BeginProperty(rect, label, property);
            
            rect.width -= EditorGUIUtility.singleLineHeight + DividerDefault;
            EditorGUI.PropertyField(rect, eventNameProp);
            
            EditorGUI.EndProperty();

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
            rect.x += rect.width + DividerGroup;
            rect.width = subIdWidth;

            subIdProperty.intValue = EditorGUI.IntField(rect, GUIContent.none, Application.isPlaying ? subIdProperty.intValue : 0);
            
            rect.x += subIdWidth + DividerDefault;
            rect.width = EditorGUIUtility.singleLineHeight;
            
            if (GUI.Button(rect, "▶")) {
                new EventReference(eventDomain, eventIdProp.intValue, subIdProperty.intValue).Raise();
            }

            if (Application.isPlaying) {
                rect = new Rect(
                    position.x,
                    position.y + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight,
                    position.width,
                    EditorGUIUtility.singleLineHeight
                );
                
                GUI.Label(rect, RaisedEventsLabel);

                var raisedEventsMap = EventBus.Main.RaisedEvents;
                
                foreach ((var e, int count) in raisedEventsMap) {
                    if (e.EventId != eventIdProp.intValue || e.EventDomain != eventDomain) continue;

                    rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                    GUI.Label(rect, $" - [{e.SubId}]: count {count}");
                }
            }
            
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (Application.isPlaying) {
                var eventDomain = property.serializedObject.targetObject as EventDomain;
                int eventId = property.FindPropertyRelative(nameof(EventDomain.EventEntry.id)).intValue;
                int count = 0;
                
                var raisedEventsMap = EventBus.Main.RaisedEvents;
                
                foreach (var e in raisedEventsMap.Keys) {
                    if (e.EventId == eventId && e.EventDomain == eventDomain) count++;
                }
                
                return (count + 2) * EditorGUIUtility.singleLineHeight + (count + 1) * EditorGUIUtility.standardVerticalSpacing;
            }
            
            return EditorGUIUtility.singleLineHeight; 
        }
    }

}
