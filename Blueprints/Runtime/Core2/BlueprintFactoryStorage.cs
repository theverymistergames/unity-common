using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintFactoryStorage : IBlueprintFactoryStorage {

        [SerializeField] private DataCell[] _factories;
        [SerializeField] private SerializedDictionary<int, int> _idToIndexMap;
        [SerializeField] private int _lastId;

        [Serializable]
        private struct DataCell {
            public int id;
            [SerializeReference] public IBlueprintFactory factory;
        }

        private readonly Dictionary<Type, int> _typeToIdMap = new Dictionary<Type, int>();
        private readonly Queue<int> _freeIndices = new Queue<int>();

        public long AddBlueprintNodeData(Type factoryType) {
            int factoryId = GetOrCreateFactory(factoryType);
            var factory = GetFactory(factoryId);

            int nodeId = factory.AddBlueprintNodeData();

            return BlueprintNodeAddress.Pack(factoryId, nodeId);
        }

        public long AddBlueprintNodeDataCopy(IBlueprintFactory factory, int id) {
            int factoryId = GetOrCreateFactory(factory.GetType());
            var targetFactory = GetFactory(factoryId);

            int nodeId = targetFactory.AddBlueprintNodeDataCopy(factory, id);

            return BlueprintNodeAddress.Pack(factoryId, nodeId);
        }

        public void RemoveBlueprintNodeData(long id) {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);

            var factory = GetFactory(factoryId);
            if (factory == null) return;

            factory.RemoveBlueprintNodeData(nodeId);
            if (factory.Count == 0) RemoveFactory(factoryId);
        }

        public string GetBlueprintNodeDataPath(int factoryId, int nodeId) {
#if UNITY_EDITOR
            if (_idToIndexMap == null) {
                Debug.LogWarning($"{nameof(BlueprintFactoryStorage)}: " +
                                 $"trying to get factory by id {factoryId}, " +
                                 $"but factory with this id is not found: " +
                                 $"index map is null.");

                return null;
            }

            if (!_idToIndexMap.TryGetValue(factoryId, out int index)) {
                Debug.LogWarning($"{nameof(BlueprintFactoryStorage)}: " +
                                 $"trying to get factory by id {factoryId}, " +
                                 $"but factory with this id is not found: " +
                                 $"index map has no entry with id {factoryId}.");
                return null;
            }

            if (index < 0 || index >= _factories.Length) {
                Debug.LogWarning($"{nameof(BlueprintFactoryStorage)}: " +
                                 $"trying to get factory by id {factoryId}, " +
                                 $"but factory with this id is not found: " +
                                 $"index map returned incorrect index {index}. " +
                                 $"Array size is {_factories.Length}.");

                return null;
            }

            ref var dataCell = ref _factories[index];
            var factory = dataCell.factory;

            return $"{nameof(_factories)}.Array.data[{index}].{nameof(DataCell.factory)}.{factory.GetBlueprintNodeDataPath(nodeId)}";
#endif

            throw new InvalidOperationException($"{nameof(BlueprintFactoryStorage)}: " +
                                                $"calling method {nameof(GetBlueprintNodeDataPath)} is only allowed in the Unity Editor.");
        }

        public void Clear() {
            _factories = null;
            _idToIndexMap = null;
            _lastId = 0;
            _freeIndices.Clear();
        }

        public void OptimizeDataLayout() {
            OptimizeFactoriesLayout();

            for (int i = 0; i < _factories.Length; i++) {
                ref var dataCell = ref _factories[i];
                dataCell.factory.OptimizeDataLayout();
            }
        }

        private int GetOrCreateFactory(Type factoryType) {
            if (_typeToIdMap.TryGetValue(factoryType, out int id)) return id;

            var instance = Activator.CreateInstance(factoryType) as IBlueprintFactory;
            id = AddFactory(instance);

            _typeToIdMap.Add(factoryType, id);

            return id;
        }

        public IBlueprintFactory GetFactory(int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_idToIndexMap == null) {
                Debug.LogWarning($"{nameof(BlueprintFactoryStorage)}: " +
                                 $"trying to get factory by id {id}, " +
                                 $"but factory with this id is not found: " +
                                 $"index map is null.");

                return null;
            }
#endif

            if (!_idToIndexMap.TryGetValue(id, out int index)) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"{nameof(BlueprintFactoryStorage)}: " +
                                 $"trying to get factory by id {id}, " +
                                 $"but factory with this id is not found: " +
                                 $"index map has no entry with id {id}.");
#endif
                return null;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= _factories.Length) {
                Debug.LogWarning($"{nameof(BlueprintFactoryStorage)}: " +
                                 $"trying to get factory by id {id}, " +
                                 $"but factory with this id is not found: " +
                                 $"index map returned incorrect index {index}. " +
                                 $"Array size is {_factories.Length}.");

                return null;
            }
#endif

            ref var dataCell = ref _factories[index];
            return dataCell.factory;
        }

        private int AddFactory(IBlueprintFactory factory) {
            _factories ??= new DataCell[1];
            _idToIndexMap ??= new SerializedDictionary<int, int>(1);

            _lastId++;
            if (_lastId == 0) _lastId++;

            int id = _lastId;
            int index = _freeIndices.TryDequeue(out int freeIndex) ? freeIndex : _idToIndexMap.Count;

            _idToIndexMap.Add(id, index);
            ArrayExtensions.EnsureCapacity(ref _factories, index + 1);

            ref var dataCell = ref _factories[index];

            dataCell.id = id;
            dataCell.factory = factory;

            return id;
        }

        private void RemoveFactory(int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_idToIndexMap == null) {
                Debug.LogWarning($"{nameof(BlueprintFactoryStorage)}: " +
                                 $"trying to remove factory by id {id}, " +
                                 $"but factory with this id is not found: " +
                                 $"index map is null.");

                return;
            }
#endif

            if (!_idToIndexMap.TryGetValue(id, out int index)) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"{nameof(BlueprintFactoryStorage)}: " +
                                 $"trying to remove factory by id {id}, " +
                                 $"but factory with this id is not found: " +
                                 $"index map has no entry with id {id}.");
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= _factories.Length) {
                Debug.LogError($"{nameof(BlueprintFactoryStorage)}: " +
                               $"trying to remove factory by id {id}, " +
                               $"but factory with this id is not found: " +
                               $"index map returned incorrect index {index}. " +
                               $"Array size is {_factories.Length}.");

                return;
            }
#endif

            ref var dataCell = ref _factories[index];

            dataCell.factory.Clear();

            dataCell.id = 0;
            dataCell.factory = null;

            _idToIndexMap.Remove(id);
            _freeIndices.Enqueue(index);

            if (_freeIndices.Count < _idToIndexMap.Count * 0.5f) return;

            _freeIndices.Clear();
            OptimizeFactoriesLayout();
        }

        private void OptimizeFactoriesLayout() {
            int offset = 0;

            for (int i = 0; i < _factories.Length; i++) {
                ref var cellA = ref _factories[i];

                if (cellA.id == 0) {
                    offset++;
                    continue;
                }

                if (offset == 0) continue;

                int newIndex = i - offset;
                ref var cellB = ref _factories[newIndex];

                cellB = cellA;
                _idToIndexMap[cellA.id] = newIndex;
            }

            Array.Resize(ref _factories, _idToIndexMap.Count);
        }
    }

}
