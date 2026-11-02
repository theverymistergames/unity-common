using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace MisterGames.Logic.Editor.Transforms {

    [InitializeOnLoad]
    public static class PhysicsStepTool {

        private const float HOLD_DELAY = 0.3f;
        private const float MIN_STEPS_PER_SECOND = 20f;
        private const float MAX_STEPS_PER_SECOND = 600f;
        private const float RAMP_DURATION = 2f;
        private const int MAX_STEPS_PER_UPDATE = 50;
        private const int MAX_HISTORY = 20000;

        private struct BodyState {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 linearVelocity;
            public Vector3 angularVelocity;
        }

        private static readonly List<Rigidbody> _bodies = new();
        private static readonly List<Rigidbody> _tempBodies = new();
        private static readonly List<MeshCollider> _convexChanged = new();
        private static readonly Dictionary<Rigidbody, bool> _kinematicRestore = new();
        private static readonly List<BodyState[]> _history = new();
        private static readonly List<Transform> _undoTransforms = new();
        private static readonly List<BodyState> _burstStartStates = new();

        private static bool _sessionActive;
        private static SimulationMode _prevSimulationMode;
        private static bool _simulationModeChanged;
        private static PhysicsScene _physicsScene;
        private static int _direction;
        private static double _holdStartTime;
        private static double _lastUpdateTime;
        private static float _stepAccumulator;

        static PhysicsStepTool() {
            Selection.selectionChanged += EndSession;
            Undo.undoRedoPerformed += EndSession;
            AssemblyReloadEvents.beforeAssemblyReload += EndSession;
            EditorApplication.playModeStateChanged += _ => EndSession();
            PrefabStage.prefabStageOpened += _ => EndSession();
            PrefabStage.prefabStageClosing += _ => EndSession();
            EditorApplication.update += OnUpdate;
        }

        [ClutchShortcut("Physics Step Tool/Step Forward", null, KeyCode.Equals, ShortcutModifiers.Shift)]
        public static void StepForward(ShortcutArguments args) => OnShortcut(args, 1);

        [ClutchShortcut("Physics Step Tool/Step Backward", null, KeyCode.Minus, ShortcutModifiers.Shift)]
        public static void StepBackward(ShortcutArguments args) => OnShortcut(args, -1);

        [ClutchShortcut("Physics Step Tool/Step Forward (Keypad)", null, KeyCode.KeypadPlus, ShortcutModifiers.Shift)]
        public static void StepForwardKeypad(ShortcutArguments args) => OnShortcut(args, 1);

        [ClutchShortcut("Physics Step Tool/Step Backward (Keypad)", null, KeyCode.KeypadMinus, ShortcutModifiers.Shift)]
        public static void StepBackwardKeypad(ShortcutArguments args) => OnShortcut(args, -1);

        private static void OnShortcut(ShortcutArguments args, int direction) {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (args.stage == ShortcutStage.Begin) {
                if (!_sessionActive && !BeginSession()) return;

                BeginBurst();

                _direction = direction;
                _holdStartTime = EditorApplication.timeSinceStartup;
                _lastUpdateTime = _holdStartTime;
                _stepAccumulator = 0f;

                Step(direction, 1);
                return;
            }

            if (_direction != direction) return;

            _direction = 0;
            CommitBurstUndo();
        }

        private static void OnUpdate() {
            if (_direction == 0 || !_sessionActive) return;

            double now = EditorApplication.timeSinceStartup;
            float dt = (float) (now - _lastUpdateTime);
            _lastUpdateTime = now;

            float elapsed = (float) (now - _holdStartTime) - HOLD_DELAY;
            if (elapsed < 0f) return;

            float t = Mathf.Clamp01(elapsed / RAMP_DURATION);
            float stepsPerSecond = Mathf.Lerp(MIN_STEPS_PER_SECOND, MAX_STEPS_PER_SECOND, t * t);

            _stepAccumulator += stepsPerSecond * dt;
            int steps = Mathf.Min((int) _stepAccumulator, MAX_STEPS_PER_UPDATE);
            _stepAccumulator -= steps;

            if (steps > 0) Step(_direction, steps);
        }

        private static bool BeginSession() {
            var transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
            if (transforms.Length == 0) return false;

            _bodies.Clear();

            for (int i = 0; i < transforms.Length; i++) {
                var root = transforms[i];

                if (root.GetComponentInChildren<Collider>() == null) {
                    Debug.LogWarning($"PhysicsStepTool: {root.name} has no colliders, skipped.");
                    continue;
                }

                var rigidbodies = root.GetComponentsInChildren<Rigidbody>();

                if (rigidbodies.Length == 0) {
                    var rb = root.gameObject.AddComponent<Rigidbody>();
                    rb.hideFlags = HideFlags.DontSave;
                    _tempBodies.Add(rb);
                    rigidbodies = new[] { rb };
                }

                _bodies.AddRange(rigidbodies);

                var meshColliders = root.GetComponentsInChildren<MeshCollider>();

                for (int j = 0; j < meshColliders.Length; j++) {
                    var meshCollider = meshColliders[j];
                    if (meshCollider.convex) continue;

                    meshCollider.convex = true;
                    _convexChanged.Add(meshCollider);
                }
            }

            if (_bodies.Count == 0) {
                RestoreSceneState();
                return false;
            }

            var simulated = new HashSet<Rigidbody>(_bodies);
            var allBodies = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Rigidbody>();

            for (int i = 0; i < allBodies.Length; i++) {
                var rb = allBodies[i];
                bool kinematic = !simulated.Contains(rb);
                if (rb.isKinematic == kinematic) continue;

                _kinematicRestore[rb] = rb.isKinematic;
                rb.isKinematic = kinematic;
            }

            for (int i = 0; i < _bodies.Count; i++) {
                var rb = _bodies[i];
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            _physicsScene = _bodies[0].gameObject.scene.GetPhysicsScene();
            _simulationModeChanged = _physicsScene == Physics.defaultPhysicsScene;

            if (_simulationModeChanged) {
                _prevSimulationMode = Physics.simulationMode;
                Physics.simulationMode = SimulationMode.Script;
            }

            _history.Clear();
            _sessionActive = true;

            return true;
        }

        private static void EndSession() {
            if (!_sessionActive) return;

            if (_direction != 0) {
                _direction = 0;
                CommitBurstUndo();
            }

            _sessionActive = false;
            _history.Clear();
            _bodies.Clear();
            _undoTransforms.Clear();
            _burstStartStates.Clear();

            if (_simulationModeChanged) Physics.simulationMode = _prevSimulationMode;
            _simulationModeChanged = false;

            RestoreSceneState();
        }

        private static void RestoreSceneState() {
            foreach ((var rb, bool kinematic) in _kinematicRestore) {
                if (rb != null) rb.isKinematic = kinematic;
            }

            for (int i = 0; i < _convexChanged.Count; i++) {
                if (_convexChanged[i] != null) _convexChanged[i].convex = false;
            }

            for (int i = 0; i < _tempBodies.Count; i++) {
                if (_tempBodies[i] != null) Object.DestroyImmediate(_tempBodies[i]);
            }

            _kinematicRestore.Clear();
            _convexChanged.Clear();
            _tempBodies.Clear();
        }

        private static void BeginBurst() {
            _undoTransforms.Clear();
            _burstStartStates.Clear();

            for (int i = 0; i < _bodies.Count; i++) {
                var rb = _bodies[i];
                if (rb == null) continue;

                var trf = rb.transform;
                _undoTransforms.Add(trf);
                _burstStartStates.Add(new BodyState { position = trf.position, rotation = trf.rotation });
            }
        }

        private static void CommitBurstUndo() {
            int count = _undoTransforms.Count;
            if (count == 0) return;

            var finalStates = new BodyState[count];
            var records = new List<Object>(count);

            for (int i = 0; i < count; i++) {
                var trf = _undoTransforms[i];
                if (trf == null) continue;

                finalStates[i] = new BodyState { position = trf.position, rotation = trf.rotation };
                trf.SetPositionAndRotation(_burstStartStates[i].position, _burstStartStates[i].rotation);
                records.Add(trf);
            }

            Undo.RecordObjects(records.ToArray(), "Physics Step");

            for (int i = 0; i < count; i++) {
                var trf = _undoTransforms[i];
                if (trf == null) continue;

                trf.SetPositionAndRotation(finalStates[i].position, finalStates[i].rotation);
            }

            _undoTransforms.Clear();
            _burstStartStates.Clear();

            Physics.SyncTransforms();
        }

        private static void Step(int direction, int count) {
            if (direction > 0) {
                Physics.SyncTransforms();

                for (int i = 0; i < count; i++) {
                    if (_history.Count >= MAX_HISTORY) _history.RemoveAt(0);

                    _history.Add(CaptureState());
                    _physicsScene.Simulate(Time.fixedDeltaTime);
                }
            }
            else {
                if (_history.Count == 0) return;

                int target = Mathf.Max(0, _history.Count - count);

                ApplyState(_history[target]);
                _history.RemoveRange(target, _history.Count - target);
                Physics.SyncTransforms();
            }

            SceneView.RepaintAll();
        }

        private static BodyState[] CaptureState() {
            var states = new BodyState[_bodies.Count];

            for (int i = 0; i < _bodies.Count; i++) {
                var rb = _bodies[i];
                if (rb == null) continue;

                var trf = rb.transform;

                states[i] = new BodyState {
                    position = trf.position,
                    rotation = trf.rotation,
                    linearVelocity = rb.linearVelocity,
                    angularVelocity = rb.angularVelocity,
                };
            }

            return states;
        }

        private static void ApplyState(BodyState[] states) {
            for (int i = 0; i < _bodies.Count; i++) {
                var rb = _bodies[i];
                if (rb == null) continue;

                var state = states[i];
                rb.transform.SetPositionAndRotation(state.position, state.rotation);
                rb.linearVelocity = state.linearVelocity;
                rb.angularVelocity = state.angularVelocity;
            }
        }
    }
    
}