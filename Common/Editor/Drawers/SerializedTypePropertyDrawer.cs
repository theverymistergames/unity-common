using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(SerializedType))]
    public class SerializedTypePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            string typeAsString = property.FindPropertyRelative("_type").stringValue;
            var type = SerializedType.DeserializeType(typeAsString);

            EditorGUI.LabelField(position, label, new GUIContent(type?.Name ?? "null"));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
