using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace MisterGames.Common.Pooling {
    
    [DefaultExecutionOrder(-1000)]
    public sealed class PrefabPool : MonoBehaviour, IPrefabPool {
        
        [SerializeField] private bool _isMainPool = true;
        [SerializeField] [Min(0)] private int _defaultInitialSize;
        [SerializeField] [Min(0)] private int _defaultMaxSize;
        
        [SerializeField] private PoolSettings[] _predefinedPools;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLogs;
        
        [HideInInspector]
        [SerializeField] private int _lastPredefinedPoolsCount;
        
        [Serializable]
        private struct PoolSettings {
            [HideLabel] public string name;
            public bool enabled;
            [Min(0)] public int initialSize;
            [Min(0)] public int maxSize;
            public GameObject[] prefabs;
        }

        public static IPrefabPool Main { get; private set; }
        
        public Transform ActiveSceneRoot { get; private set; }
        public Transform PoolRoot { get; private set; }

        private static CancellationToken DestroyToken;
        
        private readonly Dictionary<int, IObjectPool<GameObject>> _poolMap = new();
        private readonly List<GameObject> _activeSceneRoots = new();
        
        private void Awake() {
            if (_isMainPool) Main = this;
            DestroyToken = destroyCancellationToken;
            
            PoolRoot = transform;
            InitializePools();
        }

        private void OnDestroy() {
            if (_isMainPool) Main = null;
            DeInitializePools();
        }

        private void OnEnable() {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable() {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene oldActiveScene, Scene newActiveScene) {
            _activeSceneRoots.Clear();
            newActiveScene.GetRootGameObjects(_activeSceneRoots);

            ActiveSceneRoot = _activeSceneRoots[1].transform;
        }

        private void InitializePools() {
            for (int i = 0; i < _predefinedPools.Length; i++) {
                var poolSettings = _predefinedPools[i];
                if (!poolSettings.enabled) continue;

                for (int j = 0; j < poolSettings.prefabs.Length; j++) {
                    var prefab = poolSettings.prefabs[j];
                    if (prefab == null) continue;

                    var pool = CreatePool(prefab, poolSettings.initialSize, poolSettings.maxSize);
                    
                    _poolMap[GetPoolId(prefab)] = pool;
                    FillPool(pool, poolSettings.initialSize);
                }
            }
        }

        private void DeInitializePools() {
            _poolMap.Clear();
        }
        
        private IObjectPool<GameObject> CreatePool(GameObject prefab, int initialSize, int maxSize) {
#if UNITY_EDITOR
            if (_showDebugLogs) Debug.Log($"{nameof(PrefabPool)} {name}: frame {Time.frameCount}, create pool for {prefab}, size {initialSize} / {maxSize}");
#endif
            
            return new ObjectPool<GameObject>(
                () => CreatePoolObject(prefab),
                OnGetFromPool,
                OnReleaseToPool,
                DestroyPoolObject,
                collectionCheck: false,
                initialSize,
                maxSize
            );
        }

        private void FillPool(IObjectPool<GameObject> pool, int count) {
            for (int i = 0; i < count; i++) { 
                pool.Get().SetActive(false);
            }
        }
        
        public GameObject Get(GameObject prefab, bool active = true) {
            return GetInternal(prefab, PoolRoot, position: default, rotation: default, active, worldPositionStays: false, setupPositionAndRotation: false);
        }

        public GameObject Get(GameObject prefab, Transform parent, bool active = true, bool worldPositionStays = true) {
            return GetInternal(prefab, parent, position: default, rotation: default, active, worldPositionStays, setupPositionAndRotation: false);
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, bool active = true) {
            return GetInternal(prefab, PoolRoot, position, rotation, active, worldPositionStays: false, setupPositionAndRotation: true);
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) {
            return GetInternal(prefab, parent, position, rotation, active, worldPositionStays: false, setupPositionAndRotation: true);
        }

        public T Get<T>(GameObject prefab, bool active = true) where T : Component {
            return GetInternal(prefab, PoolRoot, position: default, rotation: default, active, worldPositionStays: false, setupPositionAndRotation: false).GetComponent<T>();
        }

        public T Get<T>(GameObject prefab, Transform parent, bool active = true, bool worldPositionStays = true) where T : Component {
            return GetInternal(prefab, parent, position: default, rotation: default, active, worldPositionStays, setupPositionAndRotation: false).GetComponent<T>();
        }

        public T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, bool active = true) where T : Component {
            return GetInternal(prefab, PoolRoot, position, rotation, active, worldPositionStays: false, setupPositionAndRotation: true).GetComponent<T>();
        }

        public T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) where T : Component {
            return GetInternal(prefab, parent, position, rotation, active, worldPositionStays: false, setupPositionAndRotation: true).GetComponent<T>();
        }

        public T Get<T>(T prefab, bool active = true) where T : Component {
            return GetInternal(prefab.gameObject, PoolRoot, position: default, rotation: default, active, worldPositionStays: false, setupPositionAndRotation: false).GetComponent<T>();
        }

        public T Get<T>(T prefab, Transform parent, bool active = true, bool worldPositionStays = true) where T : Component {
            return GetInternal(prefab.gameObject, parent, position: default, rotation: default, active, worldPositionStays, setupPositionAndRotation: false).GetComponent<T>();
        }

        public T Get<T>(T prefab, Vector3 position, Quaternion rotation, bool active = true) where T : Component {
            return GetInternal(prefab.gameObject, PoolRoot, position, rotation, active, worldPositionStays: false, setupPositionAndRotation: true).GetComponent<T>();
        }

        public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) where T : Component {
            return GetInternal(prefab.gameObject, parent, position, rotation, active, worldPositionStays: false, setupPositionAndRotation: true).GetComponent<T>();
        }

        public void Release(GameObject instance, float duration = 0f) {
            if (instance == null) return;
            
            WaitAndRelease(instance, duration, DestroyToken).Forget();
        }

        public void Release(Component component, float duration = 0f) {
            if (component == null) return;
            
            WaitAndRelease(component.gameObject, duration, DestroyToken).Forget();
        }
        
        private GameObject GetInternal(
            GameObject prefab,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            bool active,
            bool worldPositionStays,
            bool setupPositionAndRotation
        ) {
            if (prefab == null) return null;
            
#if UNITY_EDITOR
            if (_showDebugLogs) Debug.Log($"{nameof(PrefabPool)} {name}: frame {Time.frameCount}, get instance of prefab {prefab}");
#endif
            
            var instance = _poolMap.GetValueOrDefault(GetPoolId(prefab))?.Get();
            if (instance == null) instance = CreatePoolObject(prefab);
            
            var t = instance.transform;
            t.SetParent(parent, worldPositionStays);
            if (setupPositionAndRotation) t.SetPositionAndRotation(position, rotation);

            instance.SetActive(active);
            
            if (instance.TryGetComponent(out PoolElement poolElement)) {
                poolElement.NotifyTakenFromPool(this);
            }
            
            return instance;
        }

        private async UniTask WaitAndRelease(GameObject instance, float duration, CancellationToken cancellationToken) {
            if (duration > 0f) {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
            }
            
            if (cancellationToken.IsCancellationRequested) return;

#if UNITY_EDITOR
            if (_showDebugLogs) Debug.Log($"{nameof(PrefabPool)} {name}: frame {Time.frameCount}, release instance {instance}");
#endif
            
            if (instance.TryGetComponent(out PoolElement poolElement)) {
                poolElement.NotifyReleasedToPool(this);
            }
            
            if (_poolMap.TryGetValue(GetPoolId(instance), out var pool)) {
                pool.Release(instance);
            }
            else {
                DestroyPoolObject(instance);   
            }
        }

        private GameObject CreatePoolObject(GameObject prefab) {
            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity, PoolRoot);
            instance.name = prefab.name;
            
            return instance;
        }

        private void DestroyPoolObject(GameObject go) {
            Destroy(go);
        }

        private void OnGetFromPool(GameObject go) {
            
        }

        private void OnReleaseToPool(GameObject go) {
            go.SetActive(false);
        }
        
        private static int GetPoolId(GameObject instance) {
            return Animator.StringToHash(instance.name);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            for (int i = _lastPredefinedPoolsCount; i < _predefinedPools?.Length; i++) {
                ref var poolSettings = ref _predefinedPools[i];

                poolSettings.name = null;
                poolSettings.enabled = true;
                poolSettings.initialSize = _defaultInitialSize;
                poolSettings.maxSize = _defaultMaxSize;
                poolSettings.prefabs = new GameObject[1];
            }

            _lastPredefinedPoolsCount = _predefinedPools?.Length ?? 0;
        }
#endif
    }
    
}