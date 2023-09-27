using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Base class for deriving user defined factories with data.
    /// </summary>
    /// <typeparam name="TData">Struct data type to store in the data array.</typeparam>
    [Serializable]
    public abstract class BlueprintFactory<TData> : IBlueprintFactory where TData : struct {

        [SerializeField] private DataCell[] _array;
        [SerializeField] private SerializedDictionary<int, int> _idToIndexMap;
        [SerializeField] private int _lastId;

        [Serializable]
        private struct DataCell {
            public int id;
            public TData data;
        }

        public IBlueprintNode Node => _node ??= CreateNode();
        public int Count => _idToIndexMap?.Count ?? 0;

        public static TData Default;
        private readonly Queue<int> _freeIndices = new Queue<int>();
        private IBlueprintNode _node;

        public abstract IBlueprintNode CreateNode();

        public ref T GetData<T>(int id) where T : struct {
            if (this is not BlueprintFactory<T> factory) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"{nameof(BlueprintFactory<TData>)}: " +
                               $"trying to get value of type {typeof(T).Name} by id {id}, " +
                               $"but requested data is of type {typeof(TData).Name}. ");
#endif
                return ref BlueprintFactory<T>.Default;
            }

            var idToIndexMap = factory._idToIndexMap;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (idToIndexMap == null) {
                Debug.LogWarning($"{nameof(BlueprintFactory<TData>)}: " +
                               $"trying to get value of type {typeof(T).Name} by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map is null.");

                return ref BlueprintFactory<T>.Default;
            }
#endif

            if (!idToIndexMap.TryGetValue(id, out int index)) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"{nameof(BlueprintFactory<TData>)}: " +
                               $"trying to get value of type {typeof(T).Name} by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map has no entry with id {id}.");
#endif
                return ref BlueprintFactory<T>.Default;
            }

            var array = factory._array;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= array.Length) {
                Debug.LogError($"{nameof(BlueprintFactory<TData>)}: " +
                               $"trying to get value of type {typeof(T).Name} by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map returned incorrect index {index}. " +
                               $"Array size is {array.Length}.");

                return ref BlueprintFactory<T>.Default;
            }
#endif

            ref var dataCell = ref array[index];
            return ref dataCell.data;
        }

        public int AddBlueprintNodeData() {
            _array ??= new DataCell[1];
            _idToIndexMap ??= new SerializedDictionary<int, int>(1);

            _lastId++;
            if (_lastId == 0) _lastId++;

            int id = _lastId;
            int index = _freeIndices.TryDequeue(out int freeIndex) ? freeIndex : _idToIndexMap.Count;

            _idToIndexMap.Add(id, index);
            ArrayExtensions.EnsureCapacity(ref _array, index + 1);

            ref var dataCell = ref _array[index];

            dataCell.id = id;
            dataCell.data = default;

            return id;
        }

        public int AddBlueprintNodeDataCopy(IBlueprintFactory factory, int id) {
            int localId = AddBlueprintNodeData();
            ref var localData = ref GetData<TData>(localId);

            localData = factory.GetData<TData>(id);

            return localId;
        }

        public void RemoveBlueprintNodeData(int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_idToIndexMap == null) {
                Debug.LogWarning($"{nameof(BlueprintFactory<TData>)}: " +
                               $"trying to remove value by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map is null.");

                return;
            }
#endif

            if (!_idToIndexMap.TryGetValue(id, out int index)) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"{nameof(BlueprintFactory<TData>)}: " +
                               $"trying to remove value by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map has no entry with id {id}.");
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= _array.Length) {
                Debug.LogError($"{nameof(BlueprintFactory<TData>)}: " +
                               $"trying to remove value by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map returned incorrect index {index}. " +
                               $"Array size is {_array.Length}.");

                return;
            }
#endif

            ref var dataCell = ref _array[index];
            dataCell.id = 0;

            _idToIndexMap.Remove(id);
            _freeIndices.Enqueue(index);

            if (_freeIndices.Count < _idToIndexMap.Count * 0.5f) return;

            _freeIndices.Clear();
            OptimizeDataLayout();
        }

        public string GetBlueprintNodeDataPath(int id) {
#if UNITY_EDITOR
            if (_idToIndexMap == null) {
                Debug.LogError($"{nameof(BlueprintFactory<TData>)}: " +
                                 $"trying to get element path by id {id}, " +
                                 $"but data with this id is not found: " +
                                 $"index map is null.");

                return null;
            }

            if (!_idToIndexMap.TryGetValue(id, out int index)) {
                Debug.LogError($"{nameof(BlueprintFactory<TData>)}: " +
                                 $"trying to get element path by id {id}, " +
                                 $"but data with this id is not found: " +
                                 $"index map has no entry with id {id}.");

                return null;
            }

            if (index < 0 || index >= _array.Length) {
                Debug.LogError($"{nameof(BlueprintFactory<TData>)}: " +
                               $"trying to get element path by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map returned incorrect index {index}. " +
                               $"Array size is {_array.Length}.");

                return null;
            }

            return $"{nameof(_array)}.Array.data[{index}].{nameof(DataCell.data)}";
#endif

            throw new InvalidOperationException($"{nameof(BlueprintFactory<TData>)}: " +
                                                $"calling method {nameof(GetBlueprintNodeDataPath)} is only allowed in the Unity Editor.");
        }

        public void Clear() {
            _array = null;
            _idToIndexMap = null;
            _lastId = 0;
            _freeIndices.Clear();
        }

        public void OptimizeDataLayout() {
            int offset = 0;

            for (int i = 0; i < _array.Length; i++) {
                ref var cellA = ref _array[i];

                if (cellA.id == 0) {
                    offset++;
                    continue;
                }

                if (offset == 0) continue;

                int newIndex = i - offset;
                ref var cellB = ref _array[newIndex];

                cellB = cellA;
                _idToIndexMap[cellA.id] = newIndex;
            }

            Array.Resize(ref _array, _idToIndexMap.Count);
        }
    }

    /// <summary>
    /// Base class for deriving user defined factories without data.
    /// </summary>
    [Serializable]
    public abstract class BlueprintFactory : IBlueprintFactory {

        public IBlueprintNode Node => _node ??= CreateNode();
        public int Count => 0;

        private IBlueprintNode _node;

        public abstract IBlueprintNode CreateNode();

        public ref T GetData<T>(int id) where T : struct {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"{nameof(BlueprintFactory)}: " +
                           $"trying to get value of type {typeof(T).Name} by id {id}, " +
                           $"but this factory has no storage for data.");
#endif
            return ref BlueprintFactory<T>.Default;
        }

        public int AddBlueprintNodeData() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"{nameof(BlueprintFactory)}: " +
                             $"trying to add node data, " +
                             $"but this factory has no storage for data.");
#endif
            return -1;
        }

        public int AddBlueprintNodeDataCopy(IBlueprintFactory factory, int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"{nameof(BlueprintFactory)}: " +
                             $"trying to add node data, " +
                             $"but this factory has no storage for data.");
#endif
            return -1;
        }

        public void RemoveBlueprintNodeData(int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"{nameof(BlueprintFactory)}: " +
                             $"trying to remove node data with id {id}, " +
                             $"but this factory has no storage for data.");
#endif
        }

        public string GetBlueprintNodeDataPath(int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"{nameof(BlueprintFactory)}: " +
                             $"trying to get node data path, " +
                             $"but this factory has no storage for data.");
#endif
            return null;
        }

        public void Clear() { }

        public void OptimizeDataLayout() { }
    }

}
