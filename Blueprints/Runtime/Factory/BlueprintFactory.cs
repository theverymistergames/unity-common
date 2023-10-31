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
            return _sources.TryGetValue(id, out var source) ? source : null;
        }

        public int GetOrCreateSource(Type sourceType) {
#if UNITY_EDITOR
            foreach (int key in _sources.Keys) {
                if (_sources[key].GetType() == sourceType) return key;
            }

            if (Activator.CreateInstance(sourceType) is not IBlueprintSource instance) {
                throw new ArgumentException($"{nameof(BlueprintFactory)}: can not create source of type {sourceType}.");
            }

            return AddSource(instance);
#else
            if (_typeToIdMap.TryGetValue(sourceType, out int id)) return id;

            if (Activator.CreateInstance(sourceType) is not IBlueprintSource instance) {
                throw new ArgumentException($"{nameof(BlueprintFactory)}: can not create source of type {sourceType}.");
            }

            id = AddSource(instance);
            _typeToIdMap.Add(sourceType, id);

            return id;
#endif
        }

        public void RemoveSource(int id) {
            _sources.Remove(id);
        }

        public string GetNodePath(NodeId id) {
#if UNITY_EDITOR
            if (!_sources.TryGetValue(id.source, out var source) || source == null) {
                Debug.LogWarning($"{nameof(BlueprintFactory)}: " +
                                 $"trying to get source by id {id.source}, " +
                                 $"but source with this id is not found: " +
                                 $"map has no entry with id {id.source}.");
                return null;
            }

            return $"{nameof(_sources)}._nodes.Array.data[{_sources.IndexOf(id.source)}].value.{source.GetNodePath(id.node)}";
#endif

            throw new InvalidOperationException($"{nameof(BlueprintFactory)}: " +
                                                $"calling method {nameof(GetNodePath)} is only allowed in the Unity Editor.");
        }

        public void Clear() {
            _sources.Clear();
            _typeToIdMap.Clear();
            _lastId = 0;
        }

        private int AddSource(IBlueprintSource source) {
            int id = _lastId++;
            _sources.Add(id, source);
            return id;
        }

        public override string ToString() {
            return $"{nameof(BlueprintFactory)}: sources {_sources}";
        }
    }

}
