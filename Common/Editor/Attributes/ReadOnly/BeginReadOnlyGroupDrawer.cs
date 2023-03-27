using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes.ReadOnly {

    [CustomPropertyDrawer(typeof(BeginReadOnlyGroupAttribute))]
    public class BeginReadOnlyGroupDrawer : DecoratorDrawer {

        public override void OnGUI(Rect position) {
            var readOnlyAttribute = (BeginReadOnlyGroupAttribute) attribute;
            bool disableGui = ReadOnlyUtils.IsDisabledGui(readOnlyAttribute.mode);

            EditorGUI.BeginDisabledGroup(disableGui);
        }

        public override float GetHeight() => 0f;
    }

}
