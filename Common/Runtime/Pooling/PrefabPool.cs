using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Strings;
using UnityEngine;
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
        
        public Transform ActiveSceneRoot { get; private set; }
        public Transform PoolRoot { get; private set; }
        
        private readonly Dictionary<int, IObjectPoolAsync<GameObject>> _poolMap = new();
        private readonly Dictionary<int, AutoPoolUsage> _autoPoolUsageMap = new();
        private readonly List<GameObject> _activeSceneRoots = new();

        private CancellationTokenSource _destroyCts;
        private Transform _disabledRoot;
        private bool _isEnabled;
        
        private void Awake() {
            if (_isMainPool) Main = this;

            AsyncExt.RecreateCts(ref _destroyCts);
            _isEnabled = true;

            PoolRoot = transform;
            
            CreateDisabledRoot();
            InitializePools();

            StartAutoPoolChecks(_destroyCts.Token).Forget();
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);
            
            DeInitializePools();
            if (_isMainPool) Main = null;
        }

        private void OnEnable() {
            _isEnabled = true;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable() {
            _isEnabled = false;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void CreateDisabledRoot() {
            var disabledRoot = new GameObject("DisabledRoot");
            disabledRoot.SetActive(false);
            _disabledRoot = disabledRoot.transform;
            _disabledRoot.SetParent(PoolRoot, false);
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

            switch (_activeSceneRoots.Count) {
                case 0:
                    ActiveSceneRoot = null;
                    return;
                
                case 1:
                    ActiveSceneRoot = _activeSceneRoots[0].transform;
                    return;
            }

            for (int i = 1; i < _activeSceneRoots.Count; i++) {
                var root = _activeSceneRoots[i];
                if (!root.activeSelf || !root.activeInHierarchy) continue;
                
                ActiveSceneRoot = root.transform;
                return;
            }
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
                    
                    FillPool(pool, prefab, poolSettings.initialSize, _destroyCts.Token).Forget();
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
        
        private IObjectPoolAsync<GameObject> CreatePool(GameObject prefab, int initialSize, int maxSize) {
#if UNITY_EDITOR
            if (showDebugLogs) Log($"creating {(_autoPoolUsageMap.ContainsKey(GetPoolId(prefab)) ? "auto " : "")}pool for {prefab}, size {initialSize}/{maxSize}");
#endif
            
            return new ObjectPoolAsync<GameObject>(
                CreatePoolObject,
                CreatePoolObjectAsync,
                OnGetFromPool,
                OnReleaseToPool,
                DestroyPoolObject,
                collectionCheck: false,
                initialSize,
                maxSize
            );
        }

        private static async UniTask FillPool(IObjectPoolAsync<GameObject> pool, GameObject prefab, int count, CancellationToken cancellationToken) {
            for (int i = 0; i < count && !cancellationToken.IsCancellationRequested; i++) { 
                pool.Release(await pool.GetAsync(prefab));
            }
        }
        
        public GameObject Get(GameObject prefab, bool active = true) {
            return !_isEnabled || prefab is null ? null 
                : GetInternal(prefab, PoolRoot, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: false);
        }

        public GameObject Get(GameObject prefab, Transform parent, bool active = true, bool worldPositionStays = true) {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, parent, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays, setupPositionAndRotation: false);
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, bool active = true) {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, PoolRoot, position, rotation, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: true);
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, bool active = true) {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, PoolRoot, position, rotation, scale, active, worldPositionStays: false, setupPositionAndRotation: true);
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, parent, position, rotation, prefab.transform.localScale, active, worldPositionStays: true, setupPositionAndRotation: true);
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, bool active = true) {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, parent, position, rotation, scale, active, worldPositionStays: true, setupPositionAndRotation: true);
        }

        public T Get<T>(GameObject prefab, bool active = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, PoolRoot, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: false).GetComponent<T>();
        }

        public T Get<T>(GameObject prefab, Transform parent, bool active = true, bool worldPositionStays = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, parent, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays, setupPositionAndRotation: false).GetComponent<T>();
        }

        public T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, bool active = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, PoolRoot, position, rotation, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: true).GetComponent<T>();
        }

        public T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, bool active = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, PoolRoot, position, rotation, scale, active, worldPositionStays: false, setupPositionAndRotation: true).GetComponent<T>();
        }

        public T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, parent, position, rotation, prefab.transform.localScale, active, worldPositionStays: true, setupPositionAndRotation: true).GetComponent<T>();
        }

        public T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, bool active = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab, parent, position, rotation, scale, active, worldPositionStays: true, setupPositionAndRotation: true).GetComponent<T>();
        }

        public T Get<T>(T prefab, bool active = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab.gameObject, PoolRoot, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: false).GetComponent<T>();
        }

        public T Get<T>(T prefab, Transform parent, bool active = true, bool worldPositionStays = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab.gameObject, parent, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays, setupPositionAndRotation: false).GetComponent<T>();
        }

        public T Get<T>(T prefab, Vector3 position, Quaternion rotation, bool active = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab.gameObject, PoolRoot, position, rotation, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: true).GetComponent<T>();
        }

        public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Vector3 scale, bool active = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab.gameObject, PoolRoot, position, rotation, scale, active, worldPositionStays: false, setupPositionAndRotation: true).GetComponent<T>();
        }

        public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab.gameObject, parent, position, rotation, prefab.transform.localScale, active, worldPositionStays: true, setupPositionAndRotation: true).GetComponent<T>();
        }

        public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, bool active = true) where T : Component {
            return !_isEnabled || prefab is null ? null
                : GetInternal(prefab.gameObject, parent, position, rotation, scale, active, worldPositionStays: true, setupPositionAndRotation: true).GetComponent<T>();
        }
        
        public UniTask<GameObject> GetAsync(GameObject prefab, bool active = true) {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<GameObject>(null) 
                : GetInternalAsync(prefab, PoolRoot, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: false);
        }

        public UniTask<GameObject> GetAsync(GameObject prefab, Transform parent, bool active = true, bool worldPositionStays = true) {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<GameObject>(null) 
                : GetInternalAsync(prefab, parent, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays, setupPositionAndRotation: false);
        }

        public UniTask<GameObject> GetAsync(GameObject prefab, Vector3 position, Quaternion rotation, bool active = true) {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<GameObject>(null) 
                : GetInternalAsync(prefab, PoolRoot, position, rotation, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: true);
        }

        public UniTask<GameObject> GetAsync(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, bool active = true) {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<GameObject>(null) 
                : GetInternalAsync(prefab, PoolRoot, position, rotation, scale, active, worldPositionStays: false, setupPositionAndRotation: true);
        }

        public UniTask<GameObject> GetAsync(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<GameObject>(null) 
                : GetInternalAsync(prefab, parent, position, rotation, prefab.transform.localScale, active, worldPositionStays: true, setupPositionAndRotation: true);
        }

        public UniTask<GameObject> GetAsync(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, bool active = true) {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<GameObject>(null) 
                : GetInternalAsync(prefab, parent, position, rotation, scale, active, worldPositionStays: true, setupPositionAndRotation: true);
        }

        public UniTask<T> GetAsync<T>(GameObject prefab, bool active = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab, PoolRoot, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: false);
        }

        public UniTask<T> GetAsync<T>(GameObject prefab, Transform parent, bool active = true, bool worldPositionStays = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab, parent, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays, setupPositionAndRotation: false);
        }

        public UniTask<T> GetAsync<T>(GameObject prefab, Vector3 position, Quaternion rotation, bool active = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab, PoolRoot, position, rotation, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: true);
        }

        public UniTask<T> GetAsync<T>(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, bool active = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab, PoolRoot, position, rotation, scale, active, worldPositionStays: false, setupPositionAndRotation: true);
        }

        public UniTask<T> GetAsync<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab, parent, position, rotation, prefab.transform.localScale, active, worldPositionStays: true, setupPositionAndRotation: true);
        }

        public UniTask<T> GetAsync<T>(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, bool active = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab, parent, position, rotation, scale, active, worldPositionStays: true, setupPositionAndRotation: true);
        }

        public UniTask<T> GetAsync<T>(T prefab, bool active = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab.gameObject, PoolRoot, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: false);
        }

        public UniTask<T> GetAsync<T>(T prefab, Transform parent, bool active = true, bool worldPositionStays = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab.gameObject, parent, position: default, rotation: default, prefab.transform.localScale, active, worldPositionStays, setupPositionAndRotation: false);
        }

        public UniTask<T> GetAsync<T>(T prefab, Vector3 position, Quaternion rotation, bool active = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab.gameObject, PoolRoot, position, rotation, prefab.transform.localScale, active, worldPositionStays: false, setupPositionAndRotation: true);
        }

        public UniTask<T> GetAsync<T>(T prefab, Vector3 position, Quaternion rotation, Vector3 scale, bool active = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab.gameObject, PoolRoot, position, rotation, scale, active, worldPositionStays: false, setupPositionAndRotation: true);
        }

        public UniTask<T> GetAsync<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab.gameObject, parent, position, rotation, prefab.transform.localScale, active, worldPositionStays: true, setupPositionAndRotation: true);
        }

        public UniTask<T> GetAsync<T>(T prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, bool active = true) where T : Component {
            return !_isEnabled || prefab is null 
                ? UniTask.FromResult<T>(null) 
                : GetInternalAsyncComponent<T>(prefab.gameObject, parent, position, rotation, scale, active, worldPositionStays: true, setupPositionAndRotation: true);
        }

        public void Release(GameObject instance, float duration = 0f) {
            if (!_isEnabled || instance == null) return;
            
            WaitAndRelease(instance, duration, _destroyCts.Token).Forget();
        }

        public void Release(Component component, float duration = 0f) {
            if (!_isEnabled || component == null) return;
            
            WaitAndRelease(component.gameObject, duration, _destroyCts.Token).Forget();
        }
        
        private GameObject GetInternal(
            GameObject prefab,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            bool active,
            bool worldPositionStays,
            bool setupPositionAndRotation
        ) {
            int id = GetPoolId(prefab);
            UpdateAutoPoolUsageOnTake(id, prefab);

            var instance = _poolMap.GetValueOrDefault(id)?.Get(prefab) ?? CreatePoolObject(prefab);

            var t = instance.transform;
            t.localScale = scale;
            
            t.SetParent(parent, worldPositionStays);
            if (setupPositionAndRotation) t.SetPositionAndRotation(position, rotation);

            instance.SetActive(active);
            
            if (instance.TryGetComponent(out PoolElement poolElement)) {
                poolElement.NotifyTakenFromPool(this);
            }
            
#if UNITY_EDITOR
            if (showDebugLogs) Log($"taken instance of prefab {prefab} {GetPoolInfo(id)}");
#endif
            
            return instance;
        }
        
        private async UniTask<GameObject> GetInternalAsync(
            GameObject prefab,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            bool active,
            bool worldPositionStays,
            bool setupPositionAndRotation)
        {
            int id = GetPoolId(prefab);
            UpdateAutoPoolUsageOnTake(id, prefab);
            
            var instance = _poolMap.GetValueOrDefault(id) is { } pool 
                ? await pool.GetAsync(prefab) 
                : await CreatePoolObjectAsync(prefab);
            
            var t = instance.transform;
            t.localScale = scale;
            
            t.SetParent(parent, worldPositionStays);
            if (setupPositionAndRotation) t.SetPositionAndRotation(position, rotation);

            instance.SetActive(active);
            
            if (instance.TryGetComponent(out PoolElement poolElement)) {
                poolElement.NotifyTakenFromPool(this);
            }
            
#if UNITY_EDITOR
            if (showDebugLogs) Log($"taken instance of prefab {prefab} {GetPoolInfo(id)}");
#endif
            
            return instance;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask<T> GetInternalAsyncComponent<T>(
            GameObject prefab,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            bool active,
            bool worldPositionStays,
            bool setupPositionAndRotation)
            where T : Component
        {
            return (await GetInternalAsync(prefab, parent, position, rotation, scale, active, worldPositionStays, setupPositionAndRotation)).GetComponent<T>();
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
            if (showDebugLogs) Log($"released instance {instance} {GetPoolInfo(id)}");
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
        
        private void UpdateAutoPoolUsageOnRelease(int id, IObjectPoolAsync<GameObject> pool) {
            if (!_autoPoolUsageMap.TryGetValue(id, out var usage) && pool != null) return;
            
            usage.activeObjectsCount = Mathf.Max(0, usage.activeObjectsCount - 1);
            usage.lastUseTime = Time.realtimeSinceStartup;
            
            _autoPoolUsageMap[id] = usage;
        }

        private GameObject CreatePoolObject(GameObject prefab) {
            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity, _disabledRoot);
            
            instance.name = prefab.name;
            instance.SetActive(false);
            
            return instance;
        }
        
        private async UniTask<GameObject> CreatePoolObjectAsync(GameObject prefab) {
            var instance = (await InstantiateAsync(prefab, 1, _disabledRoot, Vector3.zero, Quaternion.identity))[0];
            
            instance.name = prefab.name;
            instance.SetActive(false);
            
            return instance;
        }

        private void DestroyPoolObject(GameObject go) {
            Destroy(go);
        }

        private void OnGetFromPool(GameObject go) {
            
        }

        private void OnReleaseToPool(GameObject go) {
            ReleaseToPoolAsync(go).Forget();
        }
        
        private async UniTaskVoid ReleaseToPoolAsync(GameObject go) {
            go.SetActive(false);
            
            // Wait frame to avoid setting parent while object is being deactivated/destroyed.
            await UniTask.Yield();

            if (!_isEnabled || go == null) return;
            
            go.transform.SetParent(PoolRoot);
        }
        
        private static int GetPoolId(GameObject instance) {
            return Animator.StringToHash(instance.name);
        }

        private void Log(string message) {
            Debug.Log($"{$"PrefabPool {(_isMainPool ? "Main" : name)}".FormatColorOnlyForEditor(Color.cyan)} [f {Time.frameCount}]: {message}");
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