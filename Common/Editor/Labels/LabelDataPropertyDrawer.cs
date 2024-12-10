using MisterGames.Common.Labels;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(LabelLibrary.LabelData))]
    public sealed class LabelDataPropertyDrawer : PropertyDrawer {
        
        private const string NamePropertyPath = nameof(LabelLibrary.LabelData.name);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property.FindPropertyRelative(NamePropertyPath), label);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
    
}