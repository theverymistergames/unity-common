using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {

    [CustomPropertyDrawer(typeof(EventDomain.EventEntry))]
    public class EventEntryPropertyDrawer : PropertyDrawer {

        private static readonly GUIContent RaisedEventsLabel = new GUIContent("Raised events");
        private static readonly GUIContent SaveLabel = new GUIContent("Save");

        private const float DividerDefault = 3f;
        private const float DividerGroup = 60f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var eventDomain = property.serializedObject.targetObject as EventDomain;
            int eventId = property.FindPropertyRelative("id").intValue;
            
            float subIdWidth = position.width * 0.1f;
            float saveWidth = position.width * 0.1f;
            
            var rect = position;
            
            rect.width -= EditorGUIUtility.singleLineHeight + DividerDefault + DividerGroup + subIdWidth;
            rect.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.BeginProperty(rect, label, property);
            
            rect.width -= EditorGUIUtility.singleLineHeight + DividerDefault + saveWidth;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("name"), GUIContent.none);
            
            rect.x += rect.width + DividerDefault;
            rect.width = saveWidth;
            GUI.Label(rect, SaveLabel);
            
            rect.x += rect.width + DividerDefault;
            rect.width = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("save"), GUIContent.none);
            EditorGUI.EndProperty();

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
            rect.x += rect.width + DividerGroup;
            rect.width = subIdWidth;

            var subIdProperty = property.FindPropertyRelative("subId");
            subIdProperty.intValue = EditorGUI.IntField(rect, GUIContent.none, subIdProperty.intValue);
            
            rect.x += subIdWidth + DividerDefault;
            rect.width = EditorGUIUtility.singleLineHeight;
            
            if (GUI.Button(rect, "▶")) {
                new EventReference(eventDomain, eventId, subIdProperty.intValue).Raise();
            }

            if (Application.isPlaying) {
                rect = new Rect(
                    position.x,
                    position.y + EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight,
                    position.width,
                    EditorGUIUtility.singleLineHeight
                );
                
                GUI.Label(rect, RaisedEventsLabel);

                var raisedEventsMap = ((EventSystem) EventSystem.Main).RaisedEvents;
                
                foreach ((var e, int count) in raisedEventsMap) {
                    if (e.EventId != eventId || e.EventDomain != eventDomain) continue;

                    rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                    GUI.Label(rect, $" - [{e.SubId}]: count {count}");
                }
            }
            
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (Application.isPlaying) {
                var eventDomain = property.serializedObject.targetObject as EventDomain;
                int eventId = property.FindPropertyRelative("id").intValue;
                int count = 0;
                
                var raisedEventsMap = ((EventSystem) EventSystem.Main).RaisedEvents;
                
                foreach (var e in raisedEventsMap.Keys) {
                    if (e.EventId == eventId && e.EventDomain == eventDomain) count++;
                }
                
                return (count + 2) * EditorGUIUtility.singleLineHeight + (count + 1) * EditorGUIUtility.standardVerticalSpacing;
            }
            
            return EditorGUIUtility.singleLineHeight; 
        }
    }

}
