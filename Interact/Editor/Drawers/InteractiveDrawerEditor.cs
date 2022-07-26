using MisterGames.Interact.Objects;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Drawers {

    [CustomEditor(typeof(InteractiveDrawer), true)]
    public class InteractiveDrawerEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var drawer = target as InteractiveDrawer;
            if (drawer == null) return;
            
            if (GUILayout.Button("Save current position as opened")) {
                drawer.SaveCurrentPositionAsOpened();
            }
            
            if (GUILayout.Button("Save current position as closed")) {
                drawer.SaveCurrentPositionAsClosed();
            }
            
            if (GUILayout.Button("Open")) {
                drawer.SetCurrentPositionOpened();
            }
            
            if (GUILayout.Button("Close")) {
                drawer.SetCurrentPositionClosed();
            }
        }
        
    }
}