using MisterGames.UI.UiServices;
using UnityEditor;
using UnityEngine;

namespace MisterGames.UI.Editor.Drawers {

    [CustomPropertyDrawer(typeof(CursorIcon))]
    public sealed class CursorIconDrawer : PropertyDrawer {

        private const float Spacing = 2f;
        private const float HotspotWidthRatio = 0.4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var rect = EditorGUI.PrefixLabel(position, label);
            
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float hotspotWidth = rect.width * HotspotWidthRatio;
            var iconRect = new Rect(rect.x, rect.y, rect.width - hotspotWidth - Spacing, EditorGUIUtility.singleLineHeight);
            var hotspotRect = new Rect(iconRect.xMax + Spacing, rect.y, hotspotWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(iconRect, property.FindPropertyRelative(nameof(CursorIcon.icon)), GUIContent.none);
            EditorGUI.PropertyField(hotspotRect, property.FindPropertyRelative(nameof(CursorIcon.hotspot)), GUIContent.none);

            EditorGUI.indentLevel = indent;
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }
    
}
