using System;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(SerializedTypePropertyDrawer))]
    public class SerializedTypePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            string typeText = property.GetValue() is SerializedType t ? ((Type) t).Name : "null";

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.LabelField(position, label, new GUIContent(typeText));
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
