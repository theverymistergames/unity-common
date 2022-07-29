using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Pooling {

    public class PrefabPool : MonoBehaviour {

        [Header("Default pool settings")]
        [SerializeField] [Min(0)] private int _initialCapacity = 0;
        [SerializeField] [Range(0f, 1f)] private float _ensureCapacityAt = 0.3f;
        [SerializeField] [Range(1f, 3f)] private float _ensureCapacityCoeff = 1.7f;

        [SerializeField] private PoolLauncher[] _poolLaunchers;

        [Serializable]
        private struct PoolLauncher
        {
            public GameObject prefab;
            [Min(0)] public int initialCapacity;
            [Range(0f, 1f)] public float ensureCapacityAt;
            [Range(1f, 3f)] public float ensureCapacityCoeff;
        }

        public static PrefabPool Instance { get; private set; }

        private Dictionary<int, ObjectPool<GameObject>> _pools;
        private List<int> _prefabIds;
        private IPoolFactory<GameObject> _factory;

        private void Awake() {
            _factory = new ParentedGameObjectFactory(transform);

            int poolsCount = _poolLaunchers.Length;
            _pools = new Dictionary<int, ObjectPool<GameObject>>(poolsCount);
            _prefabIds = new List<int>(poolsCount);

            for (int i = 0; i < poolsCount; i++) {
                var launcher = _poolLaunchers[i];
                int prefabId = GetPrefabId(launcher.prefab);

                _prefabIds.Add(prefabId);
                _pools[prefabId] = new ObjectPool<GameObject>(launcher.prefab, _factory);
            }

            Instance = this;
        }

        private void OnEnable() {
            for (int i = 0; i < _prefabIds.Count; i++) {
                int prefabId = _prefabIds[i];
                var launcher = _poolLaunchers[i];

                _pools[prefabId].Initialize(launcher.ensureCapacityAt, launcher.ensureCapacityCoeff, launcher.initialCapacity);
            }
        }

        private void OnDisable() {
            for (int i = 0; i < _prefabIds.Count; i++) {
                int prefabId = _prefabIds[i];
                _pools[prefabId].Clear();
            }
            _pools.Clear();
            _prefabIds.Clear();
        }

        private void OnDestroy() {
            Instance = null;
        }

        public GameObject TakeActive(GameObject prefab) {
            int prefabId = GetPrefabId(prefab);

            if (_pools.ContainsKey(prefabId)) {
                return _pools[prefabId].TakeActive();
            }

            var newInstance = _factory.CreatePoolElement(prefab);
            _factory.ActivatePoolElement(newInstance);
            return newInstance;
        }

        public GameObject TakeInactive(GameObject prefab) {
            int prefabId = GetPrefabId(prefab);

            if (_pools.ContainsKey(prefabId)) {
                return _pools[prefabId].TakeInactive();
            }

            var newInstance = _factory.CreatePoolElement(prefab);
            _factory.DeactivatePoolElement(newInstance);
            return newInstance;
        }

        public void Recycle(GameObject element) {
            int prefabId = GetPrefabId(element);

            if (_pools.ContainsKey(prefabId)) {
                _pools[prefabId].Recycle(element);
                return;
            }

            _factory.DeactivatePoolElement(element);
            _factory.DestroyPoolElement(element);
        }

        private static int GetPrefabId(GameObject prefab) {
            return prefab.name.GetHashCode();
        }

#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField] private string[] _searchPrefabsInFolders;
        public string[] SearchPrefabsInFolders => _searchPrefabsInFolders;

        public void Refresh(GameObject[] newPrefabs)
        {
            var lastItems = _poolLaunchers;
            int oldCount = lastItems.Length;

            int newCount = newPrefabs.Length;
            _poolLaunchers = new PoolLauncher[newCount];

            for (int i = 0; i < newCount; i++)
            {
                var newPrefab = newPrefabs[i];
                string newPrefabName = newPrefab.name;

                int capacity = _initialCapacity;
                float ensureCapacityAt = _ensureCapacityAt;
                float ensureCapacityCoeff = _ensureCapacityCoeff;

                for (int p = 0; p < oldCount; p++)
                {
                    var lastItem = lastItems[p];
                    if (lastItem.prefab.name != newPrefabName)
                    {
                        continue;
                    }

                    capacity = lastItem.initialCapacity;
                    ensureCapacityAt = lastItem.ensureCapacityAt;
                    ensureCapacityCoeff = lastItem.ensureCapacityCoeff;
                    break;
                }

                _poolLaunchers[i] = new PoolLauncher
                {
                    prefab = newPrefab,
                    initialCapacity = capacity,
                    ensureCapacityAt = ensureCapacityAt,
                    ensureCapacityCoeff = ensureCapacityCoeff
                };
            }
        }
#endif
    }

}
