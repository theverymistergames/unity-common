using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(HashId))]
    public class HashIdPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var hashProperty = property.FindPropertyRelative("_hash");
            var nameProperty = property.FindPropertyRelative("_name");
            
            EditorGUI.PropertyField(position, nameProperty, label);

            string name = nameProperty.stringValue;
            if (name.StartsWith(' ') || name.EndsWith(' ')) {
                name = name.Trim();
            }

            nameProperty.stringValue = name;
            hashProperty.intValue = string.IsNullOrWhiteSpace(name) ? 0 : Animator.StringToHash(name);
            
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
