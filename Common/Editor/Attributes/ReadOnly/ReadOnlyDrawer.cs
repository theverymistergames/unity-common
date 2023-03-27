using MisterGames.Common.Attributes;
using MisterGames.Common.Editor.SerializedProperties;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes.ReadOnly {

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var readOnlyAttribute = (ReadOnlyAttribute) attribute;
            bool disableGui = ReadOnlyUtils.IsDisabledGui(readOnlyAttribute.mode);

            EditorGUI.BeginDisabledGroup(disableGui);
            CustomPropertyGUI.PropertyField(position, property, label, fieldInfo, attribute, includeChildren: true);
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return CustomPropertyGUI.GetPropertyHeight(property, label, fieldInfo, attribute, includeChildren: true);
        }
    }

}
