using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes {

    [CustomPropertyDrawer(typeof(EmbeddedInspectorAttribute))]
    public class EmbeddedInspectorDrawer : PropertyDrawer {

        private UnityEditor.Editor _embeddedEditor;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property, label, true);

            var value = property.objectReferenceValue;

            if (value == null) {
                property.isExpanded = false;
                return;
            }

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
            if (!property.isExpanded) return;

            EditorGUI.indentLevel++;

            if (_embeddedEditor == null) {
                UnityEditor.Editor.CreateCachedEditor(value, null, ref _embeddedEditor);
            }

            _embeddedEditor.OnInspectorGUI();

            EditorGUI.indentLevel--;
        }
    }

}
