using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes.ReadOnly {

    [CustomPropertyDrawer(typeof(EndReadOnlyGroupAttribute))]
    public class EndReadOnlyGroupDrawer : DecoratorDrawer {

        public override void OnGUI(Rect position) {
            EditorGUI.EndDisabledGroup();
        }

        public override float GetHeight() => 0f;
    }

}
