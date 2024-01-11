using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenario.Editor.Events {

    [CustomPropertyDrawer(typeof(EventDomain.EventGroup))]
    public class EventGroupPropertyDrawer : PropertyDrawer {

        private static readonly GUIContent Label = new GUIContent("Group");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var headerPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(headerPosition, property.FindPropertyRelative("name"), Label);

            var eventsPosition = new Rect(
                position.x,
                position.y + headerPosition.height + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                position.height - headerPosition.height - EditorGUIUtility.standardVerticalSpacing
            );

            EditorGUI.PropertyField(eventsPosition, property.FindPropertyRelative("events"));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight +
                   EditorGUIUtility.standardVerticalSpacing +
                   EditorGUI.GetPropertyHeight(property.FindPropertyRelative("events"));
        }
    }

}
