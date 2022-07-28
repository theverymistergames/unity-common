using UnityEditor;
using UnityEngine;

namespace MisterGames.Dbg.Editor.Windows {
    
    public class ScenePhysicsToolWindow : EditorWindow {

        private float _speed = 1;

        [MenuItem("MisterGames/Physics Tool")]
        private static void ShowWindow() {
            var window = GetWindow<ScenePhysicsToolWindow>();
            window.titleContent = new GUIContent("Physics Tool");
            window.Show();
        }

        private void OnGUI() {
            _speed = EditorGUILayout.Slider(new GUIContent("Speed"), _speed, 0, 10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Start")) {
                StartSimulation();
            }

            if (GUILayout.Button("Step")) {
                RunSimulationStep();
            }

            if (GUILayout.Button("Stop")) {
                StopSimulation();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Switch ragdoll")) {
                SwitchRagdoll();
            }
        }

        private void StartSimulation()
        {
            Physics.autoSimulation = false;

            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private void StopSimulation()
        {
            EditorApplication.update -= OnUpdate;
            Physics.autoSimulation = true;
        }

        private void OnUpdate()
        {
            Physics.Simulate(Time.fixedDeltaTime * _speed);
        }

        private void RunSimulationStep() {
            EditorApplication.update -= OnUpdate;

            bool autoSim = Physics.autoSimulation;
            Physics.autoSimulation = false;

            OnUpdate();

            Physics.autoSimulation = autoSim;
        }

        private static void SwitchRagdoll() {
            if (Selection.activeGameObject == null) return;

            var rigidbody = Selection.activeGameObject.GetComponent<Rigidbody>();
            rigidbody.freezeRotation = !rigidbody.freezeRotation;
        }
    }
    
}
