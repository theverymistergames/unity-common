using System;
using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer {
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
 
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var readOnlyAttribute = (ReadOnlyAttribute) attribute;

            bool disableGui = readOnlyAttribute.mode switch {
                ReadOnlyMode.Always => true,
                ReadOnlyMode.PlayModeOnly => Application.isPlaying,
                _ => throw new NotImplementedException($"Read only mode {readOnlyAttribute.mode} is not supported for ReadOnly editor.")
            };

            using (new EditorGUI.DisabledGroupScope(disableGui)) {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

    }

    [CustomPropertyDrawer(typeof(BeginReadOnlyGroupAttribute))]
    public class BeginReadOnlyGroupDrawer : DecoratorDrawer {

        public override float GetHeight() {
            return 0;
        }
 
        public override void OnGUI(Rect position) {
            EditorGUI.BeginDisabledGroup(true);
        }
 
    }
 
    [CustomPropertyDrawer(typeof(EndReadOnlyGroupAttribute))]
    public class EndReadOnlyGroupDrawer : DecoratorDrawer {

        public override float GetHeight() {
            return 0;
        }
 
        public override void OnGUI(Rect position) {
            EditorGUI.EndDisabledGroup();
        }
 
    }
    
}
