using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public sealed class BlueprintFactoryStorage : IBlueprintFactoryStorage {

        [SerializeField] private int _lastId;
        [SerializeField] private ReferenceArrayMap<int, IBlueprintFactory> _factories
            = new ReferenceArrayMap<int, IBlueprintFactory>();

        private readonly Dictionary<Type, int> _typeToIdMap = new Dictionary<Type, int>();

        public IBlueprintFactory GetFactory(int id) {
            return _factories[id];
        }

        public int GetOrCreateFactory(Type factoryType) {
#if UNITY_EDITOR
            foreach (int key in _factories.Keys) {
                if (_factories[key].GetType() == factoryType) return key;
            }

            return AddFactory(Activator.CreateInstance(factoryType) as IBlueprintFactory);
#else
            if (_typeToIdMap.TryGetValue(factoryType, out int id)) return id;

            var instance = Activator.CreateInstance(factoryType) as IBlueprintFactory;
            id = AddFactory(instance);

            _typeToIdMap.Add(factoryType, id);

            return id;
#endif
        }

        public void RemoveFactory(int id) {
            _factories.Remove(id);
        }

        public string GetFactoryPath(int id) {
#if UNITY_EDITOR
            if (!_factories.ContainsKey(id)) {
                Debug.LogWarning($"{nameof(BlueprintFactoryStorage)}: " +
                                 $"trying to get factory by id {id}, " +
                                 $"but factory with this id is not found: " +
                                 $"map has no entry with id {id}.");
                return null;
            }

            return $"{nameof(_factories)}._entries.Array.data[{_factories.IndexOf(id)}].value";
#endif

            throw new InvalidOperationException($"{nameof(BlueprintFactoryStorage)}: " +
                                                $"calling method {nameof(GetFactoryPath)} is only allowed in the Unity Editor.");
        }

        public void Clear() {
            _factories.Clear();
            _typeToIdMap.Clear();
            _lastId = 0;
        }

        private int AddFactory(IBlueprintFactory factory) {
            _lastId++;
            if (_lastId == 0) _lastId++;

            _factories.Add(_lastId, factory);

            return _lastId;
        }
    }

}
