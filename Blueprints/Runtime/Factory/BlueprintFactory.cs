using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Factory {

    [Serializable]
    public sealed class BlueprintFactory : IBlueprintFactory {

        [SerializeField] private ReferenceArrayMap<int, IBlueprintSource> _sources;
        [SerializeField] private int _lastId;

        private readonly Dictionary<Type, int> _typeToIdMap = new Dictionary<Type, int>();

        public BlueprintFactory() {
            _sources = new ReferenceArrayMap<int, IBlueprintSource>();
        }

        public IBlueprintSource GetSource(int id) {
            return _sources[id];
        }

        public int GetOrCreateSource(Type sourceType) {
#if UNITY_EDITOR
            foreach (int key in _sources.Keys) {
                if (_sources[key].GetType() == sourceType) return key;
            }

            return AddSource(Activator.CreateInstance(sourceType) as IBlueprintSource);
#else
            if (_typeToIdMap.TryGetValue(factoryType, out int id)) return id;

            var instance = Activator.CreateInstance(factoryType) as IBlueprintFactory;
            id = AddFactory(instance);

            _typeToIdMap.Add(factoryType, id);

            return id;
#endif
        }

        public void RemoveSource(int id) {
            _sources.Remove(id);
        }

        public string GetSourcePath(int id) {
#if UNITY_EDITOR
            if (!_sources.ContainsKey(id)) {
                Debug.LogWarning($"{nameof(BlueprintFactory)}: " +
                                 $"trying to get source by id {id}, " +
                                 $"but source with this id is not found: " +
                                 $"map has no entry with id {id}.");
                return null;
            }

            return $"{nameof(_sources)}._nodes.Array.data[{_sources.IndexOf(id)}].value";
#endif

            throw new InvalidOperationException($"{nameof(BlueprintFactory)}: " +
                                                $"calling method {nameof(GetSourcePath)} is only allowed in the Unity Editor.");
        }

        public void Clear() {
            _sources.Clear();
            _typeToIdMap.Clear();
            _lastId = 0;
        }

        private int AddSource(IBlueprintSource source) {
            _lastId++;
            if (_lastId == 0) _lastId++;

            _sources.Add(_lastId, source);

            return _lastId;
        }

        public override string ToString() {
            return $"{nameof(BlueprintFactory)}: sources {_sources}";
        }
    }

}
