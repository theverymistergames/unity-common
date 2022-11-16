using MisterGames.Tick.Core;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Tick.Editor.TimeDomains {
    
    [CustomEditor(typeof(TimeDomain))]
    public class TimeDomainEditor : UnityEditor.Editor {
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (!Application.isPlaying) return;
            if (target is not TimeDomain timeDomain) return;

            GUILayout.Space(4);
            DrawCurrentTimeDomainInfo(timeDomain);
            GUILayout.Space(4);
            DrawCurrentTimeDomainState(timeDomain);
            GUILayout.Space(4);
            DrawTimeDomainControlButtons(timeDomain);
        }

        private static void DrawCurrentTimeDomainInfo(TimeDomain timeDomain) {
            bool lastGuiState = GUI.enabled;
            GUI.enabled = false;

            EditorGUILayout.FloatField("Delta Time", timeDomain.Source.DeltaTime);

            GUI.enabled = lastGuiState;
        }

        private static void DrawCurrentTimeDomainState(TimeDomain timeDomain) {
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
