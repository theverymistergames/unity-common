using System;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(SerializedTypePropertyDrawer))]
    public class SerializedTypePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            string typeText = ((Type) property.GetValue()).Name;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(position, label, typeText);
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
