using System;
using System.Linq;
using MisterGames.Tick.Core;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Tick.Editor.Drawers {
    
    [CustomEditor(typeof(TimeSourcesRunner))]
    public class TimeSourcesRunnerEditor : UnityEditor.Editor {

        private static readonly GUIStyle StyleLabelTimeSourceHeader = new GUIStyle(EditorStyles.label);

        private static readonly GUIStyle StyleLabelPause = new GUIStyle(EditorStyles.label) {
            normal = { textColor = Color.yellow },
            alignment = TextAnchor.MiddleCenter
        };

        private static readonly GUIStyle StyleLabelRunning = new GUIStyle(EditorStyles.label) {
            normal = { textColor = Color.green },
            alignment = TextAnchor.MiddleCenter
        };

        private const int DELTA_TIME_BUFFER_SIZE = 30;

        private readonly float[] _deltaTimeBuffer = new float[DELTA_TIME_BUFFER_SIZE];
        private float _averageDeltaTime;
        private int _deltaTimeBufferPointer;

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (target is not ITimeSourceProvider provider) return;

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            var playerLoopStages = typeof(PlayerLoopStage).GetEnumValues().Cast<PlayerLoopStage>();
            foreach (var stage in playerLoopStages) {
                DrawTimeSource(provider.Get(stage), $"{stage}");
                GUILayout.Space(4);
            }
        }

        private void OnEditorUpdate() {
            Repaint();
        }

        private void DrawTimeSource(ITimeSource timeSource, string label) {
            GUILayout.Label(label, StyleLabelTimeSourceHeader);
            GUILayout.Space(4);

            if (!Application.isPlaying) return;

            DrawTimeSourceFrameInfo(timeSource);
            GUILayout.Space(4);
            DrawTimeSourceRunningState(timeSource);
            GUILayout.Space(4);
            DrawTimeSourceControls(timeSource);
            GUILayout.Space(4);
        }

        private void DrawTimeSourceFrameInfo(ITimeSource timeSource) {
            int bufferSize = _deltaTimeBuffer.Length;
            for (int i = 0; i < bufferSize; i++) {
                _deltaTimeBuffer[i] = i < bufferSize - 1 ? _deltaTimeBuffer[i + 1] : timeSource.DeltaTime;
            }

            if (_deltaTimeBufferPointer++ > bufferSize - 1) {
                _deltaTimeBufferPointer = 0;

                float sum = 0f;
                for (int i = 0; i < bufferSize; i++) {
                    sum += _deltaTimeBuffer[i];
                }
                _averageDeltaTime = bufferSize > 0 ? sum / bufferSize : 0f;
            }

            int fps = _averageDeltaTime > 0f ? Mathf.FloorToInt(1f / _averageDeltaTime) : 0;

            bool lastGuiState = GUI.enabled;
            GUI.enabled = false;

            EditorGUILayout.FloatField("Delta Time", _averageDeltaTime);
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
            if (timeSource.IsPaused && GUILayout.Button("Resume")) {
                timeSource.IsPaused = false;
            }
            else if (GUILayout.Button("Pause")) {
                timeSource.IsPaused = true;
            }
        }
    }
    
}
