using MisterGames.Common.Attributes;
using MisterGames.Common.Editor.SerializedProperties;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes {

    [CustomPropertyDrawer(typeof(HideLabelAttribute))]
    public sealed class HideLabelDrawer : PropertyDrawer {

        private bool _hasCachedPropertyDrawer;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            float labelWidthCache = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0f;
            
            CustomPropertyGUI.PropertyField(position, property, GUIContent.none, fieldInfo, attribute, includeChildren: true);
            
            EditorGUIUtility.labelWidth = labelWidthCache;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return CustomPropertyGUI.GetPropertyHeight(property, label, fieldInfo, attribute, includeChildren: true);
        }
    }

}
