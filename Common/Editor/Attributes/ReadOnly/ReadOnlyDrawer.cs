using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var readOnlyAttribute = (ReadOnlyAttribute) attribute;
            bool disableGui = ReadOnlyUtils.IsDisabledGui(readOnlyAttribute.mode);

            EditorGUI.BeginDisabledGroup(disableGui);
            PropertyDrawerUtils.DrawPropertyField(position, property, label, fieldInfo, attribute, includeChildren: true);
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return PropertyDrawerUtils.GetPropertyHeight(property, label, fieldInfo, attribute, includeChildren: true);
        }
    }

}
