using System.Collections.Generic;
using MisterGames.Common.Tick;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Tick.Editor.Drawers {
    
    [CustomEditor(typeof(TimeSourcesRunner))]
    public class TimeSourcesRunnerEditor : UnityEditor.Editor {

        private const int DELTA_TIME_BUFFER_SIZE = 30;

        private static GUIStyle StyleLabelTimeSourceHeader => new GUIStyle(EditorStyles.label);

        private static GUIStyle StyleLabelPause => new GUIStyle(EditorStyles.label) {
            normal = { textColor = Color.yellow },
            alignment = TextAnchor.MiddleCenter
        };

        private static GUIStyle StyleLabelRunning => new GUIStyle(EditorStyles.label) {
            normal = { textColor = Color.green },
            alignment = TextAnchor.MiddleCenter
        };

        private readonly Dictionary<PlayerLoopStage, AverageBuffer> _bufferMap = new Dictionary<PlayerLoopStage, AverageBuffer>();

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (target is not ITimeSourceProvider provider) return;

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            var playerLoopStages = PlayerLoopStages.All;
            for (int i = 0; i < playerLoopStages.Length; i++) {
                var stage = playerLoopStages[i];

                if (!_bufferMap.ContainsKey(stage)) _bufferMap.Add(stage, new AverageBuffer(DELTA_TIME_BUFFER_SIZE));

                DrawTimeSource(stage, provider.Get(stage));
                GUILayout.Space(4);
            }
        }

        private void OnEditorUpdate() {
            Repaint();
        }

        private void DrawTimeSource(PlayerLoopStage stage, ITimeSource timeSource) {
            GUILayout.Label($"{stage}", StyleLabelTimeSourceHeader);
            GUILayout.Space(4);

            if (!Application.isPlaying) return;

            DrawTimeSourceFrameInfo(timeSource, _bufferMap[stage]);
            GUILayout.Space(4);
            DrawTimeSourceRunningState(timeSource);
            GUILayout.Space(4);
            DrawTimeSourceControls(timeSource);
            GUILayout.Space(4);
        }

        private static void DrawTimeSourceFrameInfo(ITimeSource timeSource, AverageBuffer buffer) {
            buffer.AddValue(timeSource.DeltaTime);

            float averageDeltaTime = buffer.Result;
            int fps = averageDeltaTime > 0f ? Mathf.FloorToInt(1f / averageDeltaTime) : 0;

            bool lastGuiState = GUI.enabled;
            GUI.enabled = false;

            EditorGUILayout.FloatField("Delta Time", averageDeltaTime);
            EditorGUILayout.IntField("FPS", fps);

            GUI.enabled = lastGuiState;
        }

        private static void DrawTimeSourceRunningState(ITimeSource timeSource) {
            if (timeSource.IsPaused) {
                GUILayout.Label("PAUSED", StyleLabelPause);
                return;
            }

            GUILayout.Label("RUNNING", StyleLabelRunning);
        }
        
        private static void DrawTimeSourceControls(ITimeSource timeSource) {
            if (timeSource.IsPaused) {
                if (GUILayout.Button("Resume")) timeSource.IsPaused = false;
                return;
            }

            if (GUILayout.Button("Pause")) {
                timeSource.IsPaused = true;
            }
        }
    }
    
}
