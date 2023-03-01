using MisterGames.Tweens.Core;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Tweens.Editor.Drawers {
    
    [CustomEditor(typeof(TweenRunner))]
    public class TweenRunnerEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (target is not TweenRunner runner) return;

            GUILayout.Label("Controls");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("▶")) runner.Play();
            if (GUILayout.Button("ll")) runner.Pause();
            if (GUILayout.Button("l<<")) runner.Rewind();
            if (GUILayout.Button(">>l")) runner.Wind();
            GUILayout.EndHorizontal();

            GUILayout.Label("Play direction");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<-")) runner.Invert(true);
            if (GUILayout.Button("->")) runner.Invert(false);
            GUILayout.EndHorizontal();
        }
    }
    
}
