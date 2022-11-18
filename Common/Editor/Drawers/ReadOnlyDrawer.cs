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
            bool disableGui = ReadOnlyUtils.IsDisabledGui(readOnlyAttribute.mode);

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
            var readOnlyAttribute = (BeginReadOnlyGroupAttribute) attribute;
            bool disableGui = ReadOnlyUtils.IsDisabledGui(readOnlyAttribute.mode);

            EditorGUI.BeginDisabledGroup(disableGui);
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

    internal static class ReadOnlyUtils {

        public static bool IsDisabledGui(ReadOnlyMode readOnlyMode) {
            return readOnlyMode switch {
                ReadOnlyMode.Always => true,
                ReadOnlyMode.PlayModeOnly => Application.isPlaying,
                _ => throw new NotImplementedException($"Read only mode {readOnlyMode} is not supported for ReadOnly editor.")
            };
        }

    }
    
}
