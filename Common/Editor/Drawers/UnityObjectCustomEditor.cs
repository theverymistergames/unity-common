using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomEditor(typeof(Object), true), CanEditMultipleObjects]
    internal class ObjectEditor : UnityEditor.Editor
    {
        private ButtonsDrawer _buttonsDrawer;

        private void OnEnable()
        {
            _buttonsDrawer = new ButtonsDrawer(target);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            _buttonsDrawer.DrawButtons(targets);
        }
    }
}