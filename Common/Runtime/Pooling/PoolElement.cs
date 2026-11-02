using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using MisterGames.Common.Attributes;
#endif

namespace MisterGames.Common.Pooling {

    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public sealed class PoolElement : MonoBehaviour {

        [Header("Lifecycle")]
        [Tooltip("If less than zero, lifetime is infinite.")]
        [SerializeField] [Min(-1f)] private float _lifetime = -1f;
        [SerializeField] private bool _useUnscaledTime;
        [SerializeField] private SpawnMode _spawnMode = SpawnMode.PoolRoot;

        [Tooltip("These objects are spawned when lifetime is out.")]
        [SerializeField] private GameObject[] _spawnOnLifetimeOut;

        [Header("State")]
        [Tooltip("Child transforms will reset their position, rotation and scale to initial values, " +
                 "when pool element is taken from the pool. Initial values are retrieved during an awake call.")]
        [SerializeField] private bool _syncTransforms;

        [Tooltip("Child rigidbodies will reset their velocities, when pool element is taken from the pool. ")]
        [SerializeField] private bool _syncRigidbodies;

        [Tooltip("These objects are enabled and disabled based on pool element take and return state.")]
        [SerializeField] private Object[] _syncState;

        [Tooltip("These objects are disabled when pool element is taken for the first time and then replaced by prefabs for each take.")]
        [SerializeField] private ReplaceData[] _replaceGameObjectsWithPrefabs;

        [Serializable]
        private struct ReplaceData {
            public GameObject gameObject;
            public GameObject prefab;
            public ParentMode parentMode;
        }

        private enum ParentMode {
            Unchanged,
            NoParent,
            PoolRoot,
            ActiveSceneRoot,
        }

        private enum SpawnMode {
            PoolRoot,
            ActiveSceneRoot,
        }

        private readonly struct TransformData {

            public readonly Transform transform;
            public readonly Vector3 position;
            public readonly Quaternion rotation;
            public readonly Vector3 scale;

            public TransformData(Transform transform, Vector3 position, Quaternion rotation, Vector3 scale) {
                this.transform = transform;
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
            }
        }

        private readonly struct RigidbodyData {

            public readonly Rigidbody rigidbody;
            public readonly bool isKinematic;
            public readonly bool useGravity;
            public readonly RigidbodyInterpolation interpolation;
            public readonly RigidbodyConstraints constraints;
            public readonly float linearDamping;
            public readonly float angularDamping;

            public RigidbodyData(
                Rigidbody rigidbody,
                bool isKinematic,
                bool useGravity,
                RigidbodyInterpolation interpolation,
                RigidbodyConstraints constraints,
                float linearDamping,
                float angularDamping
            ) {
                this.rigidbody = rigidbody;
                this.isKinematic = isKinematic;
                this.useGravity = useGravity;
                this.interpolation = interpolation;
                this.constraints = constraints;
                this.linearDamping = linearDamping;
                this.angularDamping = angularDamping;
            }
        }

        public event Action OnTake = delegate { };
        public event Action OnRelease = delegate { };

        public bool IsActivePoolElement { get; private set; }

        /// <summary>
        /// If less than zero, lifetime is infinite.
        /// </summary>
        public float LifetimeTotal { get => _lifetime * _lifetimeMultiplier; set => SetLifetime(value); }
        public float LifetimeMultiplier { get => _lifetimeMultiplier; set => SetLifetimeMultiplier(value); }
        public float LifetimeLeft => Mathf.Max(0f, LifetimeTotal - _timer);
        public float LifetimeProgress => LifetimeTotal > 0f ? Mathf.Clamp01(_timer / LifetimeTotal) : 0f;

        private readonly List<GameObject> _replacedInstances = new();
        private IPrefabPool _pool;
        private TransformData[] _syncTransformData;
        private RigidbodyData[] _syncRigidbodyData;
        private bool[] _syncStateData;
        private float _lifetimeMultiplier = 1f;
        private float _timer;
        private byte _takeId;
        private bool _initialized;

        private void Awake() {
            Initialize();
        }

        private void Initialize() {
            if (_initialized) return;

            _initialized = true;

            FetchChildTransformsData();
            FetchRigidbodiesData();
            FetchStateData();
            DisableReplacedGameObjects();
        }

        internal void NotifyTakenFromPool(IPrefabPool pool) {
            Initialize();

            byte id = _takeId.IncrementUncheckedRef();
            _pool = pool;
            _lifetimeMultiplier = 1f;

            ReleaseReplacedInstances(pool);

            SyncState(enabled: true);
            SyncChildTransforms();
            SyncRigidbodies();

            IsActivePoolElement = true;
            OnTake.Invoke();

            ReplaceGameObjectsWithPrefabsAtFrameEnd(id, pool, destroyCancellationToken).Forget();

            if (LifetimeTotal >= 0f) WaitAndRelease(id, pool, destroyCancellationToken).Forget();
        }

        internal void NotifyReleasedToPool(IPrefabPool pool) {
            _takeId.IncrementUncheckedRef();
            _pool = null;
            _lifetimeMultiplier = 1f;

            ReleaseReplacedInstances(pool);

            SyncState(enabled: false);

            IsActivePoolElement = false;
            OnRelease.Invoke();
        }

        private void SetLifetime(float value) {
            float oldValue = LifetimeTotal;
            _lifetime = value;

            RestartLifetimeIfBecameFinite(oldValue);
        }

        private void SetLifetimeMultiplier(float value) {
            float oldValue = LifetimeTotal;
            _lifetimeMultiplier = value;

            RestartLifetimeIfBecameFinite(oldValue);
        }

        private void RestartLifetimeIfBecameFinite(float oldValue) {
            if (oldValue < 0f && LifetimeTotal >= 0f && _pool != null) {
                WaitAndRelease(_takeId, _pool, destroyCancellationToken).Forget();
            }
        }

        private float GetDeltaTime() {
            return _useUnscaledTime ? TimeSources.unscaledDeltaTime : TimeSources.deltaTime;
        }

        private async UniTask WaitAndRelease(byte id, IPrefabPool pool, CancellationToken cancellationToken) {
            if (pool == null) return;

            _timer = 0f;

            while (id == _takeId &&
                   !cancellationToken.IsCancellationRequested &&
                   _timer < LifetimeTotal)
            {
                _timer += GetDeltaTime();
                await UniTask.Yield();
            }

            if (cancellationToken.IsCancellationRequested || id != _takeId || LifetimeTotal < 0f || this == null) return;

            pool.Release(gameObject);
            SpawnOnLifetimeOut(pool);
        }

        private void FetchChildTransformsData() {
            if (!_syncTransforms) return;

            var childTransforms = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
            _syncTransformData = new TransformData[childTransforms.Length];

            for (int i = 0; i < childTransforms.Length; i++) {
                var t = childTransforms[i];
                t.GetLocalPositionAndRotation(out var pos, out var rot);
                _syncTransformData[i] = new TransformData(t, pos, rot, t.localScale);
            }
        }

        private void FetchRigidbodiesData() {
            if (!_syncRigidbodies) return;

            var rigidbodies = gameObject.GetComponentsInChildren<Rigidbody>(includeInactive: true);
            _syncRigidbodyData = new RigidbodyData[rigidbodies.Length];

            for (int i = 0; i < rigidbodies.Length; i++) {
                var rb = rigidbodies[i];
                _syncRigidbodyData[i] = new RigidbodyData(
                    rb, rb.isKinematic, rb.useGravity, rb.interpolation, rb.constraints, rb.linearDamping, rb.angularDamping
                );
            }
        }

        private void FetchStateData() {
            _syncStateData = new bool[_syncState?.Length ?? 0];

            for (int i = 0; i < _syncStateData.Length; i++) {
                _syncStateData[i] = _syncState[i].IsEnabled();
            }
        }

        private void DisableReplacedGameObjects() {
            for (int i = 0; i < _replaceGameObjectsWithPrefabs.Length; i++) {
                ref var data = ref _replaceGameObjectsWithPrefabs[i];
                data.gameObject.SetActive(false);
            }
        }

        private async UniTask ReplaceGameObjectsWithPrefabsAtFrameEnd(byte id, IPrefabPool pool, CancellationToken cancellationToken) {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            if (cancellationToken.IsCancellationRequested || id != _takeId) return;

            ReplaceGameObjectsWithPrefabs(pool);
        }

        private void ReplaceGameObjectsWithPrefabs(IPrefabPool pool) {
            for (int i = 0; i < _replaceGameObjectsWithPrefabs.Length; i++) {
                ref var data = ref _replaceGameObjectsWithPrefabs[i];

                var t = data.gameObject.transform;
                var parent = data.parentMode switch {
                    ParentMode.Unchanged => t.parent,
                    ParentMode.NoParent => null,
                    ParentMode.PoolRoot => pool.PoolRoot,
                    ParentMode.ActiveSceneRoot => pool.ActiveSceneRoot,
                    _ => throw new ArgumentOutOfRangeException()
                };

                t.GetPositionAndRotation(out var pos, out var rot);
                var go = pool.Get(data.prefab, parent, active: false);

                go.transform.SetPositionAndRotation(pos, rot);
                go.SetActive(true);

                _replacedInstances.Add(go);
            }
        }

        private void ReleaseReplacedInstances(IPrefabPool pool) {
            for (int i = 0; i < _replacedInstances.Count; i++) {
                var instance = _replacedInstances[i];

                if (instance == null ||
                    instance.TryGetComponent(out PoolElement poolElement) && poolElement.LifetimeTotal >= 0f)
                {
                    continue;
                }

                pool.Release(instance);
            }

            _replacedInstances.Clear();
        }

        private void SyncChildTransforms() {
            if (!_syncTransforms) return;

            var root = transform;

            for (int i = 0; i < _syncTransformData.Length; i++) {
                var data = _syncTransformData[i];
                if (data.transform == root) continue;

                data.transform.SetLocalPositionAndRotation(data.position, data.rotation);
                data.transform.localScale = data.scale;
            }
        }

        private void SyncRigidbodies() {
            if (!_syncRigidbodies) return;

            for (int i = 0; i < _syncRigidbodyData.Length; i++) {
                var data = _syncRigidbodyData[i];

                data.rigidbody.isKinematic = data.isKinematic;
                data.rigidbody.useGravity = data.useGravity;
                data.rigidbody.interpolation = data.interpolation;
                data.rigidbody.constraints = data.constraints;
                data.rigidbody.linearDamping = data.linearDamping;
                data.rigidbody.angularDamping = data.angularDamping;

                if (data.isKinematic) continue;

                data.rigidbody.linearVelocity = Vector3.zero;
                data.rigidbody.angularVelocity = Vector3.zero;
            }
        }

        private void SyncState(bool enabled) {
            for (int i = 0; i < _syncStateData.Length; i++) {
                _syncState[i].SetEnabled(enabled && _syncStateData[i]);
            }
        }

        private void SpawnOnLifetimeOut(IPrefabPool pool) {
            transform.GetPositionAndRotation(out var position, out var rotation);

            var parent = _spawnMode switch {
                SpawnMode.PoolRoot => pool.PoolRoot,
                SpawnMode.ActiveSceneRoot => pool.ActiveSceneRoot,
                _ => throw new ArgumentOutOfRangeException()
            };

            for (int i = 0; i < _spawnOnLifetimeOut.Length; i++) {
                pool.Get(_spawnOnLifetimeOut[i], position, rotation, parent);
            }
        }

#if UNITY_EDITOR
        [Header("Info")]
        [SerializeField] [ReadOnly] private float _infoProgress;
        [SerializeField] [ReadOnly] private float _infoLifetimeTimer;
        [SerializeField] [ReadOnly] private float _infoLifetimeTotal;

        private void LateUpdate() {
            _infoProgress = LifetimeProgress;
            _infoLifetimeTimer = _timer;
            _infoLifetimeTotal = LifetimeTotal;
        }

        private void OnValidate() {
            for (int i = 0; i < _replaceGameObjectsWithPrefabs?.Length; i++) {
                ref var data = ref _replaceGameObjectsWithPrefabs[i];
                if (data.gameObject == null || data.prefab != null) continue;

                data.prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(data.gameObject);

                if (data.prefab != null) UnityEditor.EditorGUIUtility.PingObject(data.prefab);
            }
        }
#endif
    }

}
