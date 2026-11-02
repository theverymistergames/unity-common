using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MisterGames.TweenLib {

    public sealed class ExplosionSimulation : MonoBehaviour {

        [Header("Objects")]
        [SerializeField] private Transform[] _targets;

        [Header("Explosion")]
        [SerializeField] private Transform _explosionPoint;
        [SerializeField] [Min(0f)] private float _force = 10f;
        [SerializeField] [Min(0f)] private float _radius = 5f;
        [SerializeField] private float _upwardsModifier;
        [SerializeField] private ForceMode _forceMode = ForceMode.Impulse;

        [Header("Simulation")]
        [SerializeField] [Min(0f)] private float _duration = 3f;
        [SerializeField] [Range(0f, 1f)] private float _sampleDensity = 0.5f;
        [SerializeField] private bool _freezeOtherRigidbodies = true;
        [SerializeField] [Min(0f)] private float _temporaryRigidbodyMass = 1f;

        [Header("Playback")]
        [SerializeField] [Range(0f, 1f)] private float _progress;
        [SerializeField] [Min(0f)] private float _previewSpeed = 1f;
        [SerializeField] private bool _drawPaths = true;

        [Header("Recording")]
        [SerializeField] [ReadOnly] private string _recordingInfo = "No recording";
        [SerializeField] [HideInInspector] private Transform[] _recordedTargets = Array.Empty<Transform>();
        [SerializeField] [HideInInspector] private float[] _frameTimes = Array.Empty<float>();
        [SerializeField] [HideInInspector] private Vector3[] _localPositions = Array.Empty<Vector3>();
        [SerializeField] [HideInInspector] private Quaternion[] _localRotations = Array.Empty<Quaternion>();

        public bool HasRecording => _frameTimes.Length > 0 && _recordedTargets.Length > 0;
        public float Progress => _progress;
        public float RecordedDuration => HasRecording ? _frameTimes[^1] : 0f;

        public void SetProgress(float progress) {
            _progress = Mathf.Clamp01(progress);
            ApplyProgress(_progress, markDirty: true);
        }

        private void ApplyProgress(float progress, bool markDirty) {
            if (!HasRecording) return;

            int frameCount = _frameTimes.Length;
            float time = progress * _frameTimes[frameCount - 1];

            int next = FindNextFrame(time);
            int prev = Mathf.Max(0, next - 1);

            float prevTime = _frameTimes[prev];
            float nextTime = _frameTimes[next];
            float t = nextTime > prevTime ? (time - prevTime) / (nextTime - prevTime) : 1f;

            ApplyFrames(prev, next, t, markDirty);
        }

        private int FindNextFrame(float time) {
            int low = 0;
            int high = _frameTimes.Length - 1;

            while (low < high) {
                int mid = (low + high) / 2;

                if (_frameTimes[mid] < time) low = mid + 1;
                else high = mid;
            }

            return low;
        }

        private void ApplyFrames(int prev, int next, float t, bool markDirty) {
            int targetCount = _recordedTargets.Length;

            for (int i = 0; i < targetCount; i++) {
                var target = _recordedTargets[i];
                if (target == null) continue;

                int a = prev * targetCount + i;
                int b = next * targetCount + i;

                target.SetLocalPositionAndRotation(
                    Vector3.LerpUnclamped(_localPositions[a], _localPositions[b], t),
                    Quaternion.SlerpUnclamped(_localRotations[a], _localRotations[b], t)
                );

#if UNITY_EDITOR
                if (markDirty && !Application.isPlaying) EditorUtility.SetDirty(target);
#endif
            }
        }

        private Vector3 GetExplosionPosition() {
            return _explosionPoint == null ? transform.position : _explosionPoint.position;
        }

        private void UpdateRecordingInfo() {
            _recordingInfo = HasRecording
                ? $"Targets: {_recordedTargets.Length}, frames: {_frameTimes.Length}, duration: {RecordedDuration:0.00} s"
                : "No recording";
        }

#if UNITY_EDITOR
        private struct BodyRestoreData {
            public Rigidbody rigidbody;
            public bool isKinematic;
        }

        private double _previewLastTime;
        private bool _isPreviewPlaying;

        private void OnDisable() {
            StopPreview();
        }

        private void OnValidate() {
            UpdateRecordingInfo();

            if (!Application.isPlaying && !_isPreviewPlaying) ApplyProgress(_progress, markDirty: false);
        }

        private bool CanRecord() => !HasRecording && _targets is { Length: > 0 };
        private bool CanRerecord() => HasRecording && _targets is { Length: > 0 };
        private bool CanPlayPreview() => HasRecording && !_isPreviewPlaying;
        private bool CanStopPreview() => _isPreviewPlaying;
        private bool CanUseRecording() => HasRecording;

        [Button("Record Explosion", ButtonAttribute.Mode.Editor, nameof(CanRecord))]
        private void RecordButton() {
            Record();
        }

        [Button("Re-record Explosion", ButtonAttribute.Mode.Editor, nameof(CanRerecord))]
        private void RerecordButton() {
            StopPreview();
            ApplyFrames(0, 0, 0f, markDirty: true);
            Record();
        }

        [Button("Play Preview", ButtonAttribute.Mode.Editor, nameof(CanPlayPreview))]
        private void PlayPreview() {
            if (_progress >= 1f) SetProgress(0f);

            _isPreviewPlaying = true;
            _previewLastTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= OnPreviewUpdate;
            EditorApplication.update += OnPreviewUpdate;
        }

        [Button("Stop Preview", ButtonAttribute.Mode.Editor, nameof(CanStopPreview))]
        private void StopPreview() {
            _isPreviewPlaying = false;
            EditorApplication.update -= OnPreviewUpdate;
        }

        [Button("Go To Start", ButtonAttribute.Mode.Editor, nameof(CanUseRecording))]
        private void GoToStart() {
            StopPreview();
            Undo.RecordObject(this, "Explosion Simulation Progress");
            SetProgress(0f);
        }

        [Button("Go To End", ButtonAttribute.Mode.Editor, nameof(CanUseRecording))]
        private void GoToEnd() {
            StopPreview();
            Undo.RecordObject(this, "Explosion Simulation Progress");
            SetProgress(1f);
        }

        [Button("Clear Recording", ButtonAttribute.Mode.Editor, nameof(CanUseRecording))]
        private void ClearRecording() {
            StopPreview();

            RecordUndo("Clear Explosion Recording", _recordedTargets);
            ApplyFrames(0, 0, 0f, markDirty: true);

            _recordedTargets = Array.Empty<Transform>();
            _frameTimes = Array.Empty<float>();
            _localPositions = Array.Empty<Vector3>();
            _localRotations = Array.Empty<Quaternion>();
            _progress = 0f;

            UpdateRecordingInfo();
            EditorUtility.SetDirty(this);
        }

        private void OnPreviewUpdate() {
            if (this == null || !HasRecording) {
                StopPreview();
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            float dt = (float) (now - _previewLastTime);
            _previewLastTime = now;

            float duration = RecordedDuration;
            float progress = duration > 0f ? _progress + dt * _previewSpeed / duration : 1f;

            SetProgress(progress);
            SceneView.RepaintAll();

            if (progress >= 1f) StopPreview();
        }

        private void Record() {
            var targets = CollectTargets();
            if (targets.Count == 0) {
                Debug.LogWarning($"{nameof(ExplosionSimulation)}: no valid targets to record.", this);
                return;
            }

            RecordUndo("Record Explosion", targets);

            var physicsScene = gameObject.scene.GetPhysicsScene();
            bool isDefaultPhysicsScene = physicsScene == Physics.defaultPhysicsScene;
            var prevSimulationMode = Physics.simulationMode;

            var bodies = new Rigidbody[targets.Count];
            var tempBodies = new List<Rigidbody>();
            var convexChanged = new List<MeshCollider>();
            var kinematicRestore = new List<BodyRestoreData>();

            try {
                for (int i = 0; i < targets.Count; i++) {
                    var target = targets[i];

                    if (!target.TryGetComponent(out Rigidbody rb)) {
                        rb = target.gameObject.AddComponent<Rigidbody>();
                        rb.hideFlags = HideFlags.DontSave;
                        rb.mass = _temporaryRigidbodyMass;
                        tempBodies.Add(rb);
                    }

                    bodies[i] = rb;

                    var meshColliders = target.GetComponentsInChildren<MeshCollider>();
                    for (int j = 0; j < meshColliders.Length; j++) {
                        if (meshColliders[j].convex) continue;

                        meshColliders[j].convex = true;
                        convexChanged.Add(meshColliders[j]);
                    }
                }

                var simulated = new HashSet<Rigidbody>(bodies);
                var allBodies = StageUtility.GetStageHandle(gameObject).FindComponentsOfType<Rigidbody>();

                for (int i = 0; i < allBodies.Length; i++) {
                    var rb = allBodies[i];
                    bool isSimulated = simulated.Contains(rb);
                    bool kinematic = !isSimulated && (_freezeOtherRigidbodies || rb.isKinematic);

                    if (rb.isKinematic == kinematic) continue;

                    kinematicRestore.Add(new BodyRestoreData { rigidbody = rb, isKinematic = rb.isKinematic });
                    rb.isKinematic = kinematic;
                }

                for (int i = 0; i < bodies.Length; i++) {
                    bodies[i].linearVelocity = Vector3.zero;
                    bodies[i].angularVelocity = Vector3.zero;
                }

                if (isDefaultPhysicsScene) Physics.simulationMode = SimulationMode.Script;

                Physics.SyncTransforms();

                var explosionPosition = GetExplosionPosition();
                for (int i = 0; i < bodies.Length; i++) {
                    bodies[i].AddExplosionForce(_force, explosionPosition, _radius, _upwardsModifier, _forceMode);
                }

                float dt = Time.fixedDeltaTime;
                int stepCount = Mathf.Max(1, Mathf.CeilToInt(_duration / dt));
                int sampleInterval = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(stepCount, 1f, _sampleDensity)));

                var frameTimes = new List<float>();
                var positions = new List<Vector3>();
                var rotations = new List<Quaternion>();

                CaptureFrame(targets, 0f, frameTimes, positions, rotations);

                for (int step = 1; step <= stepCount; step++) {
                    physicsScene.Simulate(dt);

                    if (step % sampleInterval == 0 || step == stepCount) {
                        CaptureFrame(targets, step * dt, frameTimes, positions, rotations);
                    }
                }

                _recordedTargets = targets.ToArray();
                _frameTimes = frameTimes.ToArray();
                _localPositions = positions.ToArray();
                _localRotations = rotations.ToArray();
            }
            finally {
                for (int i = 0; i < bodies.Length; i++) {
                    if (bodies[i] == null) continue;

                    bodies[i].linearVelocity = Vector3.zero;
                    bodies[i].angularVelocity = Vector3.zero;
                }

                if (isDefaultPhysicsScene) Physics.simulationMode = prevSimulationMode;

                for (int i = 0; i < kinematicRestore.Count; i++) {
                    var data = kinematicRestore[i];
                    if (data.rigidbody != null) data.rigidbody.isKinematic = data.isKinematic;
                }

                for (int i = 0; i < convexChanged.Count; i++) {
                    if (convexChanged[i] != null) convexChanged[i].convex = false;
                }

                for (int i = 0; i < tempBodies.Count; i++) {
                    if (tempBodies[i] != null) DestroyImmediate(tempBodies[i]);
                }
            }

            UpdateRecordingInfo();
            ApplyProgress(_progress, markDirty: true);
            Physics.SyncTransforms();

            EditorUtility.SetDirty(this);
        }

        private List<Transform> CollectTargets() {
            var result = new List<Transform>();
            var set = new HashSet<Transform>();

            for (int i = 0; i < _targets.Length; i++) {
                var target = _targets[i];
                if (target == null || !set.Add(target)) continue;

                if (target.GetComponentInChildren<Collider>() == null) {
                    Debug.LogWarning($"{nameof(ExplosionSimulation)}: {target.name} has no colliders, skipped.", target);
                    continue;
                }

                result.Add(target);
            }

            return result;
        }

        private static void CaptureFrame(
            IReadOnlyList<Transform> targets,
            float time,
            List<float> frameTimes,
            List<Vector3> positions,
            List<Quaternion> rotations
        ) {
            frameTimes.Add(time);

            for (int i = 0; i < targets.Count; i++) {
                targets[i].GetLocalPositionAndRotation(out var position, out var rotation);
                positions.Add(position);
                rotations.Add(rotation);
            }
        }

        private void RecordUndo(string name, IReadOnlyList<Transform> transforms) {
            var objects = new List<UnityEngine.Object> { this };

            for (int i = 0; i < transforms.Count; i++) {
                if (transforms[i] != null) objects.Add(transforms[i]);
            }

            Undo.RecordObjects(objects.ToArray(), name);
        }

        private void OnDrawGizmosSelected() {
            var explosionPosition = GetExplosionPosition();

            Gizmos.color = new Color(1f, 0.4f, 0f, 1f);
            Gizmos.DrawWireSphere(explosionPosition, _radius);
            Gizmos.DrawSphere(explosionPosition, 0.1f);

            if (!_drawPaths || !HasRecording) return;

            int targetCount = _recordedTargets.Length;
            int frameCount = _frameTimes.Length;

            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);

            for (int i = 0; i < targetCount; i++) {
                var target = _recordedTargets[i];
                if (target == null) continue;

                var parent = target.parent;
                var prev = ToWorld(parent, _localPositions[i]);

                for (int f = 1; f < frameCount; f++) {
                    var next = ToWorld(parent, _localPositions[f * targetCount + i]);
                    Gizmos.DrawLine(prev, next);
                    prev = next;
                }
            }
        }

        private static Vector3 ToWorld(Transform parent, Vector3 localPosition) {
            return parent == null ? localPosition : parent.TransformPoint(localPosition);
        }
#endif
    }

}
