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
        private Rigidbody[] _rigidbodies;
        private float _startTime;

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
            
            public readonly Vector3 position;
            public readonly Quaternion rotation;
            public readonly Vector3 scale;
            
            public TransformData(Vector3 position, Quaternion rotation, Vector3 scale) {
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
            }
        }

        private readonly struct RigidbodyData {
            
            public readonly bool isKinematic;
            public readonly bool useGravity;
            public readonly RigidbodyInterpolation interpolation;
            public readonly RigidbodyConstraints constraints;
            
            public RigidbodyData(bool isKinematic, bool useGravity, RigidbodyInterpolation interpolation, RigidbodyConstraints constraints) {
                this.isKinematic = isKinematic;
                this.useGravity = useGravity;
                this.interpolation = interpolation;
                this.constraints = constraints;
            }
        }
        
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
            SyncChildTransforms();
            SyncRigidbodies();
            
            OnTake.Invoke();

            ReplaceGameObjectsWithPrefabsAtFrameEnd(pool).Forget();
            
            if (_lifetime >= 0f) WaitAndRelease(pool, destroyCancellationToken).Forget();
        }

        internal void NotifyReleasedToPool(IPrefabPool pool) {
            OnRelease.Invoke();
        }

        private async UniTask WaitAndRelease(IPrefabPool pool, CancellationToken cancellationToken) {
            _startTime = Time.time;

            while (!cancellationToken.IsCancellationRequested && Time.time <= _startTime + _lifetime)
            {
                await UniTask.Yield();
            }
            
            if (cancellationToken.IsCancellationRequested) return;

            gameObject.SetActive(false);
            SpawnOnLifetimeOut(pool);
        }

        private void FetchChildTransformsData() {
            if (!_syncTransforms) return;

            var t = transform;
            int count = t.childCount;
            _syncTransformData = new TransformData[count];
            
            for (int i = 0; i < count; i++) {
                var child = t.GetChild(i);
                _syncTransformData[i] = new TransformData(child.localPosition, child.localRotation, child.localScale);   
            }
        }

        private void FetchRigidbodiesData() {
            if (!_syncRigidbodies) return;

            _rigidbodies ??= gameObject.GetComponentsInChildren<Rigidbody>(includeInactive: true);
            _syncRigidbodyData ??= new RigidbodyData[_rigidbodies.Length];
            
            for (int i = 0; i < _rigidbodies.Length; i++) {
                var rb = _rigidbodies[i];
                _syncRigidbodyData[i] = new RigidbodyData(rb.isKinematic, rb.useGravity, rb.interpolation, rb.constraints);
            }
        }
        
        private void DisableReplacedGameObjects() {
            for (int i = 0; i < _replaceGameObjectsWithPrefabs.Length; i++) {
                ref var data = ref _replaceGameObjectsWithPrefabs[i];
                data.gameObject.SetActive(false);
            }
        }

        private async UniTask ReplaceGameObjectsWithPrefabsAtFrameEnd(IPrefabPool pool) {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
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
            
            var t = transform;
            int count = t.childCount;
            
            for (int i = 0; i < count; i++) {
                var child = t.GetChild(i);
                var data = _syncTransformData[i];
                
                child.SetLocalPositionAndRotation(data.position, data.rotation);
                child.localScale = data.scale;
            }
        }

        private void SyncRigidbodies() {
            if (!_syncRigidbodies) return;

            for (int i = 0; i < _rigidbodies.Length; i++) {
                var rb = _rigidbodies[i];
                var data = _syncRigidbodyData[i];
                
                rb.isKinematic = data.isKinematic;
                rb.useGravity = data.useGravity;
                rb.interpolation = data.interpolation;
                rb.constraints = data.constraints;
                
                if (!rb.isKinematic) rb.linearVelocity = Vector3.zero;
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