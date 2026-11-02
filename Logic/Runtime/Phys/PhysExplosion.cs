using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Pooling;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MisterGames.Logic.Phys {

    public sealed class PhysExplosion : MonoBehaviour, IActorComponent {

        [Header("Rigidbodies")]
        [SerializeField] private Rigidbody[] _rigidbodies;
        [SerializeField] private bool _kinematicBeforeExplosion = true;

        [Header("Explosion")]
        [SerializeField] private Transform _explosionPoint;
        [SerializeField] private float _force = 10f;
        [SerializeField] [Min(0f)] private float _radius = 5f;
        [SerializeField] private float _upwardsModifier;
        [SerializeField] private ForceMode _forceMode = ForceMode.Impulse;

        [Header("Effects")]
        [SerializeField] private GameObject[] _explosionEffects;
        [SerializeField] [Min(0f)] private float _explosionEffectsScaleMin = 1f;
        [SerializeField] [Min(0f)] private float _explosionEffectsScaleMax = 1f;
        [SerializeField] private GameObject[] _attachedEffects;
        [SerializeField] [Min(0f)] private float _attachedEffectsScaleMin = 1f;
        [SerializeField] [Min(0f)] private float _attachedEffectsScaleMax = 1f;
        [FormerlySerializedAs("_attachedEffectsPerRigidbody")]
        [SerializeField] [Min(0)] private int _attachedEffectsPerRigidbodyMax = 1;
        [SerializeField] [Range(0f, 1f)] private float _attachedEffectsChance = 1f;

        [Header("Actions")]
        [SerializeReference] [SubclassSelector] private IActorAction _onExplode;

        private struct BodyData {
            public Vector3 position;
            public Quaternion rotation;
            public bool isKinematic;
        }

        private struct SpawnedEffect {
            public GameObject instance;
            public Transform parent;
        }

        public bool IsExploded { get; private set; }

        private readonly List<SpawnedEffect> _spawnedEffects = new();
        private BodyData[] _bodyData;
        private CancellationTokenSource _explodeCts;
        private IActor _actor;
        private int[] _effectIndices;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            _bodyData = new BodyData[_rigidbodies.Length];

            for (int i = 0; i < _rigidbodies.Length; i++) {
                var rb = _rigidbodies[i];
                if (rb == null) continue;

                rb.transform.GetLocalPositionAndRotation(out var position, out var rotation);
                _bodyData[i] = new BodyData { position = position, rotation = rotation, isKinematic = rb.isKinematic };

                if (_kinematicBeforeExplosion) rb.isKinematic = true;
            }
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _explodeCts);
            ReleaseEffects();

#if UNITY_EDITOR
            EditorRollback();
#endif
        }

        public void Explode() {
            if (IsExploded) Rollback();

            IsExploded = true;

            var point = _explosionPoint == null ? transform : _explosionPoint;
            point.GetPositionAndRotation(out var center, out var centerRotation);

            for (int i = 0; i < _rigidbodies.Length; i++) {
                var rb = _rigidbodies[i];
                if (rb == null) continue;

                if (_kinematicBeforeExplosion) rb.isKinematic = _bodyData[i].isKinematic;
                if (!rb.isKinematic) rb.AddExplosionForce(_force, center, _radius, _upwardsModifier, _forceMode);

                SpawnAttachedEffects(rb.transform);
            }

            for (int i = 0; i < _explosionEffects?.Length; i++) {
                if (_explosionEffects[i] == null) continue;

                var root = PrefabPool.Main.ActiveSceneRoot;
                var instance = PrefabPool.Main.Get(_explosionEffects[i], center, centerRotation, root);
                instance.transform.localScale = _explosionEffects[i].transform.localScale * Random.Range(_explosionEffectsScaleMin, _explosionEffectsScaleMax);

                _spawnedEffects.Add(new SpawnedEffect { instance = instance, parent = root });
            }

            AsyncExt.RecreateCts(ref _explodeCts);
            _onExplode?.Apply(_actor, _explodeCts.Token).Forget();
        }

        public void Rollback() {
            IsExploded = false;

            AsyncExt.DisposeCts(ref _explodeCts);

            ReleaseEffects();

            for (int i = 0; i < _rigidbodies.Length; i++) {
                var rb = _rigidbodies[i];
                if (rb == null) continue;

                if (!rb.isKinematic) {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                var data = _bodyData[i];
                rb.transform.SetLocalPositionAndRotation(data.position, data.rotation);
                rb.position = rb.transform.position;
                rb.rotation = rb.transform.rotation;
                rb.isKinematic = _kinematicBeforeExplosion || data.isKinematic;
            }
        }

        private void SpawnAttachedEffects(Transform parent) {
            int max = Mathf.Min(_attachedEffectsPerRigidbodyMax, _attachedEffects?.Length ?? 0);
            if (max <= 0 || Random.value >= _attachedEffectsChance) return;

            int count = Random.Range(1, max + 1);

            int total = _attachedEffects.Length;
            if (_effectIndices == null || _effectIndices.Length != total) _effectIndices = new int[total];

            for (int i = 0; i < total; i++) {
                _effectIndices[i] = i;
            }

            for (int i = 0; i < count; i++) {
                int j = Random.Range(i, total);
                (_effectIndices[i], _effectIndices[j]) = (_effectIndices[j], _effectIndices[i]);

                var prefab = _attachedEffects[_effectIndices[i]];
                if (prefab == null) continue;

                parent.GetPositionAndRotation(out var position, out var rotation);
                var instance = PrefabPool.Main.Get(prefab, position, rotation, parent);
                instance.transform.localScale = prefab.transform.localScale * Random.Range(_attachedEffectsScaleMin, _attachedEffectsScaleMax);

                _spawnedEffects.Add(new SpawnedEffect { instance = instance, parent = parent });
            }
        }

        private void ReleaseEffects() {
            for (int i = 0; i < _spawnedEffects.Count; i++) {
                var effect = _spawnedEffects[i];

                if (effect.instance != null &&
                    effect.instance.activeSelf &&
                    effect.instance.transform.parent == effect.parent)
                {
                    PrefabPool.Main?.Release(effect.instance);
                }
            }

            _spawnedEffects.Clear();
        }

        [Button(mode: ButtonAttribute.Mode.Runtime)]
        private void ExplodeButton() {
            Explode();
        }

        [Button(mode: ButtonAttribute.Mode.Runtime)]
        private void RollbackButton() {
            Rollback();
        }

#if UNITY_EDITOR
        private struct EditorKinematicData {
            public Rigidbody rigidbody;
            public bool isKinematic;
        }

        private readonly List<EditorKinematicData> _editorKinematicRestore = new();
        private BodyData[] _editorBodyData;
        private PhysicsScene _editorPhysicsScene;
        private SimulationMode _editorPrevSimulationMode;
        private bool _editorSimulationModeChanged;
        private bool _editorExploded;
        private double _editorLastTime;
        private float _editorTimeAccumulator;

        private bool CanEditorRollback() => _editorExploded;

        [Button("Explode (Editor, Physics Only)", ButtonAttribute.Mode.Editor)]
        private void EditorExplode() {
            EditorRollback();

            _editorBodyData = new BodyData[_rigidbodies.Length];
            var simulated = new HashSet<Rigidbody>();

            for (int i = 0; i < _rigidbodies.Length; i++) {
                var rb = _rigidbodies[i];
                if (rb == null) continue;

                rb.transform.GetLocalPositionAndRotation(out var position, out var rotation);
                _editorBodyData[i] = new BodyData { position = position, rotation = rotation, isKinematic = rb.isKinematic };
                simulated.Add(rb);
            }

            var allBodies = UnityEditor.SceneManagement.StageUtility.GetStageHandle(gameObject).FindComponentsOfType<Rigidbody>();

            for (int i = 0; i < allBodies.Length; i++) {
                var rb = allBodies[i];
                bool kinematic = !simulated.Contains(rb);
                if (rb.isKinematic == kinematic) continue;

                _editorKinematicRestore.Add(new EditorKinematicData { rigidbody = rb, isKinematic = rb.isKinematic });
                rb.isKinematic = kinematic;
            }

            _editorPhysicsScene = gameObject.scene.GetPhysicsScene();
            _editorSimulationModeChanged = _editorPhysicsScene == Physics.defaultPhysicsScene;

            if (_editorSimulationModeChanged) {
                _editorPrevSimulationMode = Physics.simulationMode;
                Physics.simulationMode = SimulationMode.Script;
            }

            Physics.SyncTransforms();

            var center = _explosionPoint == null ? transform.position : _explosionPoint.position;

            foreach (var rb in simulated) {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddExplosionForce(_force, center, _radius, _upwardsModifier, _forceMode);
            }

            _editorExploded = true;
            _editorLastTime = UnityEditor.EditorApplication.timeSinceStartup;
            _editorTimeAccumulator = 0f;

            UnityEditor.EditorApplication.update -= EditorSimulate;
            UnityEditor.EditorApplication.update += EditorSimulate;
        }

        [Button("Rollback (Editor)", ButtonAttribute.Mode.Editor, nameof(CanEditorRollback))]
        private void EditorRollback() {
            if (!_editorExploded) return;

            _editorExploded = false;
            UnityEditor.EditorApplication.update -= EditorSimulate;

            for (int i = 0; i < _rigidbodies.Length && i < _editorBodyData.Length; i++) {
                var rb = _rigidbodies[i];
                if (rb == null) continue;

                if (!rb.isKinematic) {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                var data = _editorBodyData[i];
                rb.transform.SetLocalPositionAndRotation(data.position, data.rotation);
            }

            for (int i = 0; i < _editorKinematicRestore.Count; i++) {
                var data = _editorKinematicRestore[i];
                if (data.rigidbody != null) data.rigidbody.isKinematic = data.isKinematic;
            }

            _editorKinematicRestore.Clear();

            if (_editorSimulationModeChanged) Physics.simulationMode = _editorPrevSimulationMode;
            _editorSimulationModeChanged = false;

            Physics.SyncTransforms();
            UnityEditor.SceneView.RepaintAll();
        }

        private void EditorSimulate() {
            if (this == null || !_editorExploded || Application.isPlaying) {
                UnityEditor.EditorApplication.update -= EditorSimulate;
                return;
            }

            double now = UnityEditor.EditorApplication.timeSinceStartup;
            _editorTimeAccumulator += (float) (now - _editorLastTime);
            _editorLastTime = now;

            float dt = Time.fixedDeltaTime;
            int steps = 0;

            while (_editorTimeAccumulator >= dt && steps < 10) {
                _editorPhysicsScene.Simulate(dt);
                _editorTimeAccumulator -= dt;
                steps++;
            }

            if (steps >= 10) _editorTimeAccumulator = 0f;

            UnityEditor.SceneView.RepaintAll();
        }

        private void Reset() {
            _rigidbodies = GetComponentsInChildren<Rigidbody>(includeInactive: true);
        }

        [Button(mode: ButtonAttribute.Mode.Editor)]
        private void CollectChildRigidbodies() {
            UnityEditor.Undo.RecordObject(this, "Collect Child Rigidbodies");
            _rigidbodies = GetComponentsInChildren<Rigidbody>(includeInactive: true);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void OnDrawGizmosSelected() {
            var center = _explosionPoint == null ? transform.position : _explosionPoint.position;

            Gizmos.color = new Color(1f, 0.4f, 0f, 1f);
            Gizmos.DrawWireSphere(center, _radius);
            Gizmos.DrawSphere(center, 0.1f);
        }
#endif
    }

}
