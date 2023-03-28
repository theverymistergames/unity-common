using MisterGames.Bezier.Extensions;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Bezier.Editor {

    [CustomEditor(typeof(SplineTool))]
    public class SplineToolEditor : UnityEditor.Editor {
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var generator = target as SplineTool;
            if (generator == null) return;

            if (GUILayout.Button("Generate next")) {
                generator.GenerateNext();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Remove last")) {
                generator.RemoveLast();
                EditorUtility.SetDirty(target);
            }
            
            if (GUILayout.Button("Clear")) {
                generator.Clear();
            }
        }
        
    }

}