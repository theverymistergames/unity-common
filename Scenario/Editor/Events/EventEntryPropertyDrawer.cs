using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {

    [CustomPropertyDrawer(typeof(EventDomain.EventEntry))]
    public class EventEntryPropertyDrawer : PropertyDrawer {

        private static readonly GUIContent Label = new GUIContent("Event");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position.width -= EditorGUIUtility.singleLineHeight + 3f;   
            
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("name"), Label);
            EditorGUI.EndProperty();

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
            position.x += position.width + 3f;
            position.width = EditorGUIUtility.singleLineHeight;
            
            if (GUI.Button(position, "▶")) {
                var e = new EventReference(property.serializedObject.targetObject as EventDomain, property.FindPropertyRelative("id").intValue);
                EventSystems.Global?.Raise(e);
            }
            
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
