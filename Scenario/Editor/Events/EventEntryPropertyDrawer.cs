using System.Text;
using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {

    [CustomPropertyDrawer(typeof(EventDomain.EventEntry))]
    public class EventEntryPropertyDrawer : PropertyDrawer {

        private static readonly GUIContent Label = new GUIContent("Event");
        private static readonly GUIContent RaisedEventsLabel = new GUIContent("Raised events");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var eventDomain = property.serializedObject.targetObject as EventDomain;
            int eventId = property.FindPropertyRelative("id").intValue;
            
            float subIdWidth = position.width * 0.1f;
            var rect = position;
            rect.width -= EditorGUIUtility.singleLineHeight + 6f + subIdWidth;
            rect.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.BeginProperty(rect, label, property);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("name"), Label);
            EditorGUI.EndProperty();

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
            rect.x += rect.width + 3f;
            rect.width = subIdWidth;

            var subIdProperty = property.FindPropertyRelative("subId");
            subIdProperty.intValue = EditorGUI.IntField(rect, GUIContent.none, subIdProperty.intValue);
            
            rect.x += subIdWidth + 3f;
            rect.width = EditorGUIUtility.singleLineHeight;
            
            if (GUI.Button(rect, "▶")) {
                new EventReference(eventDomain, eventId, subIdProperty.intValue).Raise();
            }

            if (Application.isPlaying && EventSystems.Global?.RaisedEvents is {} raisedEvents) {
                rect = new Rect(
                    position.x,
                    position.y + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight,
                    position.width,
                    EditorGUIUtility.singleLineHeight
                );
                
                GUI.Label(rect, RaisedEventsLabel);

                foreach ((var e, int count) in raisedEvents) {
                    if (e.EventId != eventId || e.EventDomain != eventDomain) continue;

                    rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                    GUI.Label(rect, $" - [{e.SubId}]: count {count}");
                }
            }
            
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (Application.isPlaying && EventSystems.Global?.RaisedEvents is {} raisedEvents) {
                var eventDomain = property.serializedObject.targetObject as EventDomain;
                int eventId = property.FindPropertyRelative("id").intValue;
                int count = 0;
                
                foreach (var e in raisedEvents.Keys) {
                    if (e.EventId == eventId && e.EventDomain == eventDomain) count++;
                }
                
                return (count + 2) * EditorGUIUtility.singleLineHeight + (count + 1) * EditorGUIUtility.standardVerticalSpacing;
            }
            
            return EditorGUIUtility.singleLineHeight; 
        }
    }

}
