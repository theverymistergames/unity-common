using MisterGames.Dbg.Console.Core;
using MisterGames.MisterGames.Dbg.Editor.Console;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomEditor(typeof(ConsoleRunner))]
    public class ConsoleRunnerEditor : UnityEditor.Editor {
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            if (target is not ConsoleRunner consoleRunner) return;

            if (GUILayout.Button("Refresh console modules")) {
                consoleRunner.RefreshModules(ConsoleUtils.GetAllConsoleModules());
                EditorUtility.SetDirty(consoleRunner);
            }
        }
    }
    
}
