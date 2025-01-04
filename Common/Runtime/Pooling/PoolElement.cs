using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.GameObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Pooling {
    
    [DefaultExecutionOrder(-10000)]
    public sealed class PoolElement : MonoBehaviour {
        
        [Header("Lifecycle")]
        [Tooltip("If less than zero, lifetime is infinite.")]
        [SerializeField] private float _lifetime = -1f;
        
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
            
            public RigidbodyData(Rigidbody rigidbody, bool isKinematic, bool useGravity, RigidbodyInterpolation interpolation, RigidbodyConstraints constraints) {
                this.rigidbody = rigidbody;
                this.isKinematic = isKinematic;
                this.useGravity = useGravity;
                this.interpolation = interpolation;
                this.constraints = constraints;
            }
        }
        
        public event Action OnTake = delegate { };
        public event Action OnRelease = delegate { };
        
        /// <summary>
        /// If less than zero, lifetime is infinite.
        /// </summary>
        public float LifetimeTotal { get => _lifetime; set => _lifetime = value; }
        public float LifetimeLeft => _startTime + _lifetime - Time.time;
        
        private CancellationTokenSource _enableCts;
        private TransformData[] _syncTransformData;
        private RigidbodyData[] _syncRigidbodyData;
        private float _startTime;
        private byte _takeId;
        
        private void Awake() {
            FetchChildTransformsData();
            FetchRigidbodiesData();
            DisableReplacedGameObjects();
        }
        
        private void OnEnable() {
            SyncState(enabled: true);
        }

        private void OnDisable() {
            SyncState(enabled: false);
        }

        internal void NotifyTakenFromPool(IPrefabPool pool) {
            byte id = ++_takeId;
            
            SyncChildTransforms();
            SyncRigidbodies();
            
            OnTake.Invoke();

            ReplaceGameObjectsWithPrefabsAtFrameEnd(id, pool, destroyCancellationToken).Forget();
            
            if (_lifetime >= 0f) WaitAndRelease(id, pool, destroyCancellationToken).Forget();
        }

        internal void NotifyReleasedToPool(IPrefabPool pool) {
            _takeId++;
            OnRelease.Invoke();
        }

        private async UniTask WaitAndRelease(byte id, IPrefabPool pool, CancellationToken cancellationToken) {
            _startTime = Time.time;

            while (!cancellationToken.IsCancellationRequested && id == _takeId && Time.time <= _startTime + _lifetime)
            {
                await UniTask.Yield();
            }
            
            if (cancellationToken.IsCancellationRequested || id != _takeId) return;

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
                _syncRigidbodyData[i] = new RigidbodyData(rb, rb.isKinematic, rb.useGravity, rb.interpolation, rb.constraints);
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
            }
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

                if (data.isKinematic) continue;
                
                data.rigidbody.linearVelocity = Vector3.zero;
                data.rigidbody.angularVelocity = Vector3.zero;
            }
        }

        private void SyncState(bool enabled) {
            for (int i = 0; i < _syncState.Length; i++) {
                _syncState[i].SetEnabled(enabled);
            }
        }

        private void SpawnOnLifetimeOut(IPrefabPool pool) {
            var t = transform;
            var position = t.position;
            var rotation = t.rotation;
            
            for (int i = 0; i < _spawnOnLifetimeOut.Length; i++) {
                pool.Get(_spawnOnLifetimeOut[i], position, rotation);
            }
        }

#if UNITY_EDITOR
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