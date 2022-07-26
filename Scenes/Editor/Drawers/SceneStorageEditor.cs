using MisterGames.Scenes.Core;
using MisterGames.Scenes.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Scenes.Editor.Drawers {
    
    [CustomEditor(typeof(ScenesStorage))]
    public class SceneStorageEditor : UnityEditor.Editor {
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Refresh Scenes")) {
                ScenesStorage.Instance.Refresh();
            }
            
            if (GUILayout.Button("Include all scenes in build settings")) {
                ScenesMenu.IncludeAllScenesInBuildSettings();
            }
        }
    }
    
}