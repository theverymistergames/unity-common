using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Base class for blueprint node factories. It is used to store factories in blueprint storages.
    /// </summary>
    [Serializable]
    public abstract class BlueprintNodeFactory : IBlueprintNodeFactory, IBlueprintStorage {


        public abstract int Count { get; }


        public abstract ref T Get<T>(int id) where T : struct;


        public abstract int AddElement();


        public abstract int AddElementCopy(IBlueprintStorage storage, int id);


        public abstract void RemoveElement(int id);


        public abstract string GetElementPath(int id);


        public abstract void Clear();


        public abstract void OptimizeDataLayout();


        public abstract IBlueprintNodeFactory CreateFactory();


        public abstract IBlueprintNode CreateNode();
    }

    /// <summary>
    /// Base class for deriving user defined factories.
    /// It has implementations for most <see cref="IBlueprintNodeFactory"/> and <see cref="IBlueprintStorage"/> methods.
    /// </summary>
    /// <typeparam name="TData">Struct data type to store in the data array.</typeparam>
    [Serializable]
    public abstract class BlueprintNodeFactory<TData> : BlueprintNodeFactory where TData : struct {

        [SerializeField] private DataCell[] _array;
        [SerializeField] private SerializedDictionary<int, int> _idToIndexMap;
        [SerializeField] private int _totalNodesAddedCount;

        [Serializable]
        private struct DataCell {
            public int id;
            public TData data;
        }

        private static TData Default;
        private readonly Queue<int> _freeIndices = new Queue<int>();

        public sealed override int Count => _idToIndexMap?.Count ?? 0;

        public sealed override ref T Get<T>(int id) where T : struct {
            if (this is not BlueprintNodeFactory<T> factory) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"{nameof(BlueprintNodeFactory<TData>)}: " +
                               $"trying to get value of type {typeof(T).Name} by id {id}, " +
                               $"but requested data is of type {typeof(TData).Name}. ");
#endif
                return ref BlueprintNodeFactory<T>.Default;
            }

            var idToIndexMap = factory._idToIndexMap;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (idToIndexMap == null) {
                Debug.LogWarning($"{nameof(BlueprintNodeFactory<TData>)}: " +
                               $"trying to get value of type {typeof(T).Name} by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map is null.");

                return ref BlueprintNodeFactory<T>.Default;
            }
#endif

            if (!idToIndexMap.TryGetValue(id, out int index)) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"{nameof(BlueprintNodeFactory<TData>)}: " +
                               $"trying to get value of type {typeof(T).Name} by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map has no entry with id {id}.");
#endif
                return ref BlueprintNodeFactory<T>.Default;
            }

            var array = factory._array;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= array.Length) {
                Debug.LogError($"{nameof(BlueprintNodeFactory<TData>)}: " +
                               $"trying to get value of type {typeof(T).Name} by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map returned incorrect index {index}. " +
                               $"Array size is {array.Length}.");

                return ref BlueprintNodeFactory<T>.Default;
            }
#endif

            ref var dataCell = ref array[index];
            return ref dataCell.data;
        }

        public sealed override int AddElement() {
            _array ??= new DataCell[1];
            _idToIndexMap ??= new SerializedDictionary<int, int>(1);

            int id = ++_totalNodesAddedCount;
            int index = _freeIndices.TryDequeue(out int freeIndex) ? freeIndex : _idToIndexMap.Count;

            _idToIndexMap.Add(id, index);
            ArrayExtensions.EnsureCapacity(ref _array, index + 1);

            ref var dataCell = ref _array[index];

            dataCell.id = id;
            dataCell.data = default;

            return id;
        }

        public sealed override int AddElementCopy(IBlueprintStorage storage, int id) {
            int localId = AddElement();
            ref var localData = ref Get<TData>(localId);

            localData = storage.Get<TData>(id);

            return localId;
        }

        public sealed override void RemoveElement(int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_idToIndexMap == null) {
                Debug.LogWarning($"{nameof(BlueprintNodeFactory<TData>)}: " +
                               $"trying to remove value by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map is null.");

                return;
            }
#endif

            if (!_idToIndexMap.TryGetValue(id, out int index)) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"{nameof(BlueprintNodeFactory<TData>)}: " +
                               $"trying to remove value by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map has no entry with id {id}.");
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index < 0 || index >= _array.Length) {
                Debug.LogError($"{nameof(BlueprintNodeFactory<TData>)}: " +
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

        public sealed override string GetElementPath(int id) {
#if UNITY_EDITOR
            if (_idToIndexMap == null) {
                Debug.LogError($"{nameof(BlueprintNodeFactory<TData>)}: " +
                                 $"trying to get element path by id {id}, " +
                                 $"but data with this id is not found: " +
                                 $"index map is null.");

                return null;
            }

            if (!_idToIndexMap.TryGetValue(id, out int index)) {
                Debug.LogError($"{nameof(BlueprintNodeFactory<TData>)}: " +
                                 $"trying to get element path by id {id}, " +
                                 $"but data with this id is not found: " +
                                 $"index map has no entry with id {id}.");

                return null;
            }

            if (index < 0 || index >= _array.Length) {
                Debug.LogError($"{nameof(BlueprintNodeFactory<TData>)}: " +
                               $"trying to get element path by id {id}, " +
                               $"but data with this id is not found: " +
                               $"index map returned incorrect index {index}. " +
                               $"Array size is {_array.Length}.");

                return null;
            }

            return $"{nameof(_array)}.Array.data[{index}].{nameof(DataCell.data)}";
#endif
            throw new InvalidOperationException($"{nameof(BlueprintNodeFactory<TData>)}: " +
                                                $"calling method {nameof(GetElementPath)} is only allowed in the Unity Editor");
        }

        public sealed override void Clear() {
            _array = null;
            _idToIndexMap = null;
            _totalNodesAddedCount = 0;
            _freeIndices.Clear();
        }

        public sealed override void OptimizeDataLayout() {
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
}
