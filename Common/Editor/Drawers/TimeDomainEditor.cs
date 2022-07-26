using MisterGames.Common.Routines;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomEditor(typeof(TimeDomain))]
    public class TimeDomainEditor : UnityEditor.Editor {
        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            
            if (!(target is TimeDomain timeDomain)) return;

            GUILayout.Space(4);
            CurrentTimeDomainState(timeDomain);
            GUILayout.Space(4);
            
            PauseResumeButtons(timeDomain);
        }

        private static void CurrentTimeDomainState(TimeDomain timeDomain) {
            if (timeDomain.IsPaused) {
                GUILayout.Label("PAUSED", StyleLabelPause());
                return;
            }

            if (!timeDomain.IsStarted) {
                GUILayout.Label("WILL RUN AT START", StyleLabelWillRunAtStart());
                return;
            }

            if (timeDomain.IsActive) {
                GUILayout.Label("RUNNING", StyleLabelRunning());
                return;
            }
            
            GUILayout.Label("STOPPED", StyleLabelStopped());
        }
        
        private static void PauseResumeButtons(TimeDomain timeDomain) {
            if (timeDomain.IsPaused) {
                if (GUILayout.Button("Resume")) timeDomain.IsPaused = false;
                return;
            }

            if (GUILayout.Button("Pause")) {
                timeDomain.IsPaused = true;
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
        
        private static GUIStyle StyleLabelWillRunAtStart() => new GUIStyle(EditorStyles.label) {
            alignment = TextAnchor.MiddleCenter
        };
        
        private static GUIStyle StyleLabelStopped() => new GUIStyle(EditorStyles.label) {
            normal = { textColor = UnityEngine.Color.red },
            alignment = TextAnchor.MiddleCenter
        };
    }
    
}