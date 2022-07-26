using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Editor.Blueprints.Editor.Utils;
using MisterGames.Fsm.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Drawers {

    [CustomEditor(typeof(BlueprintRunner), true)]
    public class BlueprintRunnerDrawer : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (!(target is IBlueprintHost host)) return;

            var asset = host.Source;
            if (asset == null) return;

            if (GUILayout.Button("Open in Blueprints Editor")) {
                if (Application.isPlaying && host.Instance != null) {
                    BlueprintsEditorWindow.OpenWindow().PopulateFromHost(target);
                }
                else {
                    BlueprintsEditorWindow.OpenWindow().PopulateFromAsset(asset);    
                }
            }
        }
        
    }
}