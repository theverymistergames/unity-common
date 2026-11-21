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

        [Header("Collect In Radius")]
        [SerializeField] private bool _collectRigidbodiesInRadius = false;
        [SerializeField] [Min(0f)] private float _collectRadius = 3f;
        [SerializeField] [Min(1)] private int _collectMaxHits = 32;
        [SerializeField] private LayerMask _collectLayerMask = 1;

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
        }

        private struct CollectedBodyData {
            public Rigidbody rigidbody;
            public Vector3 position;
            public Quaternion rotation;
        }

        private struct SpawnedEffect {
            public GameObject instance;
            public Transform parent;
        }

        public bool IsExploded { get; private set; }

        private readonly List<SpawnedEffect> _spawnedEffects = new();
        private readonly HashSet<Rigidbody> _collectedRigidbodies = new();
        private readonly List<CollectedBodyData> _collectedBodyData = new();
        private Collider[] _collectHits;
        private BodyData[] _bodyData;
        private CancellationTokenSource _explodeCts;
        private IActor _actor;
        private int[] _effectIndices;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            _bodyData = CaptureBodyData();
            if (_kinematicBeforeExplosion) SetKinematic(true);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _explodeCts);
            ReleaseEffects();

#if UNITY_EDITOR
            EditorRollback();
#endif
        }

        private BodyData[] CaptureBodyData() {
            var data = new BodyData[_rigidbodies.Length];

            for (int i = 0; i < _rigidbodies.Length; i++) {
                var rb = _rigidbodies[i];
                if (rb == null) continue;

                rb.transform.GetLocalPositionAndRotation(out var position, out var rotation);
                data[i] = new BodyData { position = position, rotation = rotation };
            }

            return data;
        }

        private void RestoreBodyData(BodyData[] data) {
            for (int i = 0; i < _rigidbodies.Length && i < data.Length; i++) {
                var rb = _rigidbodies[i];
                if (rb == null) continue;

                if (!rb.isKinematic) {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                rb.transform.SetLocalPositionAndRotation(data[i].position, data[i].rotation);
                rb.position = rb.transform.position;
                rb.rotation = rb.transform.rotation;
            }
        }

        private void SetKinematic(bool isKinematic) {
            for (int i = 0; i < _rigidbodies.Length; i++) {
                if (_rigidbodies[i] != null) _rigidbodies[i].isKinematic = isKinematic;
            }

            for (int i = 0; i < _collectedBodyData.Count; i++) {
                if (_collectedBodyData[i].rigidbody != null) _collectedBodyData[i].rigidbody.isKinematic = isKinematic;
            }
        }

        private Transform GetExplosionPoint() {
            return _explosionPoint == null ? transform : _explosionPoint;
        }

        private void ApplyExplosionForce(Rigidbody rb, Vector3 center) {
            rb.AddExplosionForce(_force, center, _radius, _upwardsModifier, _forceMode);
        }

        private void CollectRigidbodiesInRadius(PhysicsScene physicsScene, Vector3 center, List<CollectedBodyData> dest) {
            if (_collectHits == null || _collectHits.Length != _collectMaxHits) _collectHits = new Collider[_collectMaxHits];

            int count = physicsScene.OverlapSphere(center, _collectRadius, _collectHits, _collectLayerMask, QueryTriggerInteraction.Ignore);

            _collectedRigidbodies.Clear();

            for (int i = 0; i < _rigidbodies.Length; i++) {
                if (_rigidbodies[i] != null) _collectedRigidbodies.Add(_rigidbodies[i]);
            }

            for (int i = 0; i < count; i++) {
                var rb = _collectHits[i].attachedRigidbody;
                if (rb == null || !_collectedRigidbodies.Add(rb)) continue;

                rb.transform.GetLocalPositionAndRotation(out var position, out var rotation);
                dest.Add(new CollectedBodyData { rigidbody = rb, position = position, rotation = rotation });
            }

            _collectedRigidbodies.Clear();
        }

        private void RestoreCollectedBodyData(List<CollectedBodyData> data) {
            for (int i = 0; i < data.Count; i++) {
                var rb = data[i].rigidbody;
                if (rb == null) continue;

                if (!rb.isKinematic) {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                rb.transform.SetLocalPositionAndRotation(data[i].position, data[i].rotation);
                rb.position = rb.transform.position;
                rb.rotation = rb.transform.rotation;
            }
        }

        private void ExplodeRigidbody(Rigidbody rb, Vector3 center) {
            rb.isKinematic = false;
            ApplyExplosionForce(rb, center);

            SpawnAttachedEffects(rb.transform);
        }

        public void Explode() {
            if (IsExploded) Rollback();

            IsExploded = true;

            GetExplosionPoint().GetPositionAndRotation(out var center, out var centerRotation);

            for (int i = 0; i < _rigidbodies.Length; i++) {
                var rb = _rigidbodies[i];
                if (rb == null) continue;

                ExplodeRigidbody(rb, center);
            }

            if (_collectRigidbodiesInRadius) {
                CollectRigidbodiesInRadius(gameObject.scene.GetPhysicsScene(), center, _collectedBodyData);

                for (int i = 0; i < _collectedBodyData.Count; i++) {
                    ExplodeRigidbody(_collectedBodyData[i].rigidbody, center);
                }
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

            RestoreBodyData(_bodyData);
            RestoreCollectedBodyData(_collectedBodyData);
            SetKinematic(_kinematicBeforeExplosion);

            _collectedBodyData.Clear();
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
        private readonly List<CollectedBodyData> _editorCollectedBodyData = new();
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

            _editorBodyData = CaptureBodyData();
            _editorPhysicsScene = gameObject.scene.GetPhysicsScene();
            var simulated = new HashSet<Rigidbody>();

            for (int i = 0; i < _rigidbodies.Length; i++) {
                if (_rigidbodies[i] != null) simulated.Add(_rigidbodies[i]);
            }

            if (_collectRigidbodiesInRadius) {
                Physics.SyncTransforms();
                CollectRigidbodiesInRadius(_editorPhysicsScene, GetExplosionPoint().position, _editorCollectedBodyData);

                for (int i = 0; i < _editorCollectedBodyData.Count; i++) {
                    simulated.Add(_editorCollectedBodyData[i].rigidbody);
                }
            }

            var allBodies = UnityEditor.SceneManagement.StageUtility.GetStageHandle(gameObject).FindComponentsOfType<Rigidbody>();

            for (int i = 0; i < allBodies.Length; i++) {
                var rb = allBodies[i];
                bool kinematic = !simulated.Contains(rb);
                if (rb.isKinematic == kinematic) continue;

                _editorKinematicRestore.Add(new EditorKinematicData { rigidbody = rb, isKinematic = rb.isKinematic });
                rb.isKinematic = kinematic;
            }

            _editorSimulationModeChanged = _editorPhysicsScene == Physics.defaultPhysicsScene;

            if (_editorSimulationModeChanged) {
                _editorPrevSimulationMode = Physics.simulationMode;
                Physics.simulationMode = SimulationMode.Script;
            }

            Physics.SyncTransforms();

            var center = GetExplosionPoint().position;

            foreach (var rb in simulated) {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                ApplyExplosionForce(rb, center);
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

            RestoreBodyData(_editorBodyData);
            RestoreCollectedBodyData(_editorCollectedBodyData);
            _editorCollectedBodyData.Clear();

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
            var center = GetExplosionPoint().position;

            Gizmos.color = new Color(1f, 0.4f, 0f, 1f);
            Gizmos.DrawWireSphere(center, _radius);
            Gizmos.DrawSphere(center, 0.1f);

            Gizmos.color = new Color(0f, 0.8f, 1f, 1f);
            Gizmos.DrawWireSphere(center, _collectRadius);
        }
#endif
    }

}
