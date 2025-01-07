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
        
        [Header("Auto Pools")]
        [SerializeField] [Min(0)] private int _totalTakeCountToCreatePool = 3; 
        [SerializeField] [Min(0)] private int _activeCountToCreatePool = 3; 
        [SerializeField] [Min(0)] private int _autoPoolMaxSize = 100; 
        [SerializeField] [Min(0)] private float _unusedPoolClearTimeout = 300f; 
        [SerializeField] [Min(0)] private float _autoPoolCheckPeriod = 1f; 
        
        [Header("Pools")]
        [SerializeField] [Min(0)] private int _defaultInitialSize;
        [SerializeField] [Min(0)] private int _defaultMaxSize;
        [SerializeField] private PoolSettings[] _predefinedPools;
        
        [Header("Debug")]
        public bool showDebugLogs;
        
        [Serializable]
        private struct PoolSettings {
#if UNITY_EDITOR
            [HideLabel] public string name;      
#endif
            
            public bool enabled;
            [Min(0)] public int initialSize;
            [Min(0)] public int maxSize;
            public GameObject[] prefabs;
        }
        
        private struct AutoPoolUsage {
            public float lastUseTime;
            public int activeObjectsCount;
            public int totalTakeCount;
        }

        public static IPrefabPool Main { get; private set; }
        private static CancellationToken DestroyToken;
        
        public Transform ActiveSceneRoot { get; private set; }
        public Transform PoolRoot { get; private set; }
        
        private readonly Dictionary<int, ObjectPool<GameObject>> _poolMap = new();
        private readonly Dictionary<int, AutoPoolUsage> _autoPoolUsageMap = new();
        private readonly List<GameObject> _activeSceneRoots = new();

        private void Awake() {
            if (_isMainPool) Main = this;
            DestroyToken = destroyCancellationToken;
            
            PoolRoot = transform;
            InitializePools();

            StartAutoPoolChecks(DestroyToken).Forget();
        }

        private void OnDestroy() {
            DeInitializePools();
            if (_isMainPool) Main = null;
        }

        private void OnEnable() {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable() {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private async UniTask StartAutoPoolChecks(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                float time = Time.realtimeSinceStartup;
                
                foreach ((int id, var usage) in _autoPoolUsageMap) {
                    if (time < usage.lastUseTime + _unusedPoolClearTimeout ||
                        !_poolMap.TryGetValue(id, out var pool) ||
                        pool.CountInactive <= 0) 
                    {
                        continue;
                    }

                    pool.Clear();
                }
                
                await UniTask.Delay(TimeSpan.FromSeconds(_autoPoolCheckPeriod), cancellationToken: cancellationToken)
                             .SuppressCancellationThrow();
            }
        }

        private void OnActiveSceneChanged(Scene oldActiveScene, Scene newActiveScene) {
            _activeSceneRoots.Clear();
            newActiveScene.GetRootGameObjects(_activeSceneRoots);

            ActiveSceneRoot = _activeSceneRoots.Count > 1 ? _activeSceneRoots[1].transform 
                : _activeSceneRoots.Count > 0 ? _activeSceneRoots[0].transform
                : null;
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
            foreach (var pool in _poolMap.Values) {
                pool.Clear();
            }

            _autoPoolUsageMap.Clear();
            _poolMap.Clear();
        }
        
        private ObjectPool<GameObject> CreatePool(GameObject prefab, int initialSize, int maxSize) {
#if UNITY_EDITOR
            Log($"creating {(_autoPoolUsageMap.ContainsKey(GetPoolId(prefab)) ? "auto " : "")}pool for {prefab}, size {initialSize}/{maxSize}");
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
                pool.Release(pool.Get());
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
            
            int id = GetPoolId(prefab);
            UpdateAutoPoolUsageOnTake(id, prefab);
            
            var pool = _poolMap.GetValueOrDefault(id);
            var instance = pool?.Get() ?? CreatePoolObject(prefab);

            var t = instance.transform;
            t.SetParent(parent, worldPositionStays);
            if (setupPositionAndRotation) t.SetPositionAndRotation(position, rotation);
            t.localScale = prefab.transform.localScale;
            
            instance.SetActive(active);
            
            if (instance.TryGetComponent(out PoolElement poolElement)) {
                poolElement.NotifyTakenFromPool(this);
            }
            
#if UNITY_EDITOR
            Log($"taken instance of prefab {prefab} {GetPoolInfo(id)}");
#endif
            
            return instance;
        }

        private async UniTask WaitAndRelease(GameObject instance, float duration, CancellationToken cancellationToken) {
            if (duration > 0f) {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
            }
            
            if (cancellationToken.IsCancellationRequested) return;

            int id = GetPoolId(instance);
            bool hasPoolElement = instance.TryGetComponent(out PoolElement poolElement);
            if (hasPoolElement) poolElement.NotifyReleasedToPool(this);
            
            if (_poolMap.TryGetValue(id, out var pool)) {
                pool.Release(instance);
            }
            else {
                DestroyPoolObject(instance);   
            }
            
            UpdateAutoPoolUsageOnRelease(id, pool);
            
#if UNITY_EDITOR
            Log($"released instance {instance} {GetPoolInfo(id)}");
#endif
        }

        private void UpdateAutoPoolUsageOnTake(int id, GameObject prefab) {
            bool hasPool = _poolMap.ContainsKey(id);
            if (!_autoPoolUsageMap.TryGetValue(id, out var usage) && hasPool) return;
            
            usage.activeObjectsCount++;
            usage.totalTakeCount++;
            usage.lastUseTime = Time.realtimeSinceStartup;
            
            _autoPoolUsageMap[id] = usage;

            if (!hasPool && 
                (usage.totalTakeCount >= _totalTakeCountToCreatePool ||
                 usage.activeObjectsCount >= _activeCountToCreatePool)) 
            {
                _poolMap[id] = CreatePool(prefab, usage.activeObjectsCount, _autoPoolMaxSize);
            }
        }
        
        private void UpdateAutoPoolUsageOnRelease(int id, IObjectPool<GameObject> pool) {
            if (!_autoPoolUsageMap.TryGetValue(id, out var usage) && pool != null) return;
            
            usage.activeObjectsCount = Mathf.Max(0, usage.activeObjectsCount - 1);
            usage.lastUseTime = Time.realtimeSinceStartup;
            
            _autoPoolUsageMap[id] = usage;
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

        private void Log(string message) {
#if UNITY_EDITOR
            string cOpen = "<color=cyan>";
            string cClose = "</color>";
#else
            string cOpen = null;
            string cClose = null;
#endif
            
            if (showDebugLogs) Debug.Log($"{cOpen}PrefabPool {(_isMainPool ? "Main" : name)}{cClose} [f {Time.frameCount}]: {message}");
        }

        private string GetPoolInfo(int id) {
            bool hasUsage = _autoPoolUsageMap.TryGetValue(id, out var usage);
            
            return _poolMap.TryGetValue(id, out var pool)
                ? $"[Pool, active {pool.CountActive}/{pool.CountAll}{(hasUsage ? $", total taken {usage.totalTakeCount}" : "")}]"
                : $"[no pool{(hasUsage ? $", total taken {usage.totalTakeCount}, active {usage.activeObjectsCount}" : "")}]";
        }
        
#if UNITY_EDITOR
        [HideInInspector]
        [SerializeField] private int _lastPredefinedPoolsCount;
        
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