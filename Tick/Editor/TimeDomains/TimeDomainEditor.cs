using MisterGames.Tick.Core;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Tick.Editor.TimeDomains {
    
    [CustomEditor(typeof(TimeDomain))]
    public class TimeDomainEditor : UnityEditor.Editor {

        private const int DELTA_TIME_BUFFER_SIZE = 30;

        private readonly float[] _deltaTimeBuffer = new float[DELTA_TIME_BUFFER_SIZE];
        private float _averageDeltaTime;
        private int _deltaTimeBufferPointer;

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (!Application.isPlaying) return;
            if (target is not TimeDomain timeDomain) return;

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            GUILayout.Space(4);
            DrawTimeDomainFrameInfo(timeDomain);
            GUILayout.Space(4);
            DrawTimeDomainState(timeDomain);
            GUILayout.Space(4);
            DrawTimeDomainControlButtons(timeDomain);
            GUILayout.Space(4);
            DrawTimeDomainSubscribers(timeDomain);
        }

        private void OnEditorUpdate() {
            Repaint();
        }

        private void DrawTimeDomainFrameInfo(TimeDomain timeDomain) {
            bool lastGuiState = GUI.enabled;
            GUI.enabled = false;

            int bufferSize = _deltaTimeBuffer.Length;
            for (int i = 0; i < bufferSize; i++) {
                _deltaTimeBuffer[i] = i < bufferSize - 1 ? _deltaTimeBuffer[i + 1] : timeDomain.Source.DeltaTime;
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

            EditorGUILayout.FloatField("Delta Time", _averageDeltaTime);
            EditorGUILayout.IntField("FPS", fps);

            GUI.enabled = lastGuiState;
        }

        private static void DrawTimeDomainSubscribers(TimeDomain timeDomain) {
            GUILayout.Label("Subscribers");

            var subs = timeDomain.SourceApi.Subscribers;
            for (int i = 0; i < subs.Count; i++) {
                var sub = subs[i];
                GUILayout.Label($"- {sub}");
            }
        }

        private static void DrawTimeDomainState(TimeDomain timeDomain) {
            if (timeDomain.Source.IsPaused) {
                GUILayout.Label("PAUSED", StyleLabelPause());
                return;
            }

            GUILayout.Label("RUNNING", StyleLabelRunning());
        }
        
        private static void DrawTimeDomainControlButtons(TimeDomain timeDomain) {
            if (timeDomain.Source.IsPaused) {
                if (GUILayout.Button("Resume")) timeDomain.Source.IsPaused = false;
                return;
            }

            if (GUILayout.Button("Pause")) {
                timeDomain.Source.IsPaused = true;
            }
        }
        
        private static GUIStyle StyleLabelPause() => new GUIStyle(EditorStyles.label) {
            normal = { textColor = UnityEngine.Color.yellow },
            alignment = TextAnchor.MiddleCenter
        };
        
        private static GUIStyle StyleLabelRunning() => new GUIStyle(EditorStyles.label) {
            normal = { textColor = UnityEngine.Color.green },
            alignment = TextAnchor.MiddleCenter
        };
    }
    
}
