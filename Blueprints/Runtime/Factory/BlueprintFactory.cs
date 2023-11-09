using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Factory {

    [Serializable]
    public sealed class BlueprintFactory : IBlueprintFactory {

        [SerializeField] private RefArrayMap<int, IBlueprintSource> _sources;
        [SerializeField] private int _lastId;

        private readonly Dictionary<Type, int> _typeToIdMap = new Dictionary<Type, int>();

        public BlueprintFactory() {
            _sources = new RefArrayMap<int, IBlueprintSource>();
        }

        public IBlueprintSource GetSource(int id) {
            return _sources.TryGetValue(id, out var source) ? source : null;
        }

        public int GetOrCreateSource(Type sourceType) {
            if (_typeToIdMap.TryGetValue(sourceType, out int id)) return id;

            id = AddSource(Activator.CreateInstance(sourceType) as IBlueprintSource);
            _typeToIdMap.Add(sourceType, id);

            return id;
        }

        public void RemoveSource(int id) {
            if (!_sources.TryGetValue(id, out var source)) return;

            if (source != null) _typeToIdMap.Remove(source.GetType());
            _sources.Remove(id);
        }

        public bool TryGetNodePath(NodeId id, out int sourceIndex, out int nodeIndex) {
            if (!_sources.TryGetValue(id.source, out var source) ||
                source == null ||
                !source.TryGetNodePath(id.node, out nodeIndex)
            ) {
                sourceIndex = -1;
                nodeIndex = -1;
                return false;
            }

            sourceIndex = _sources.IndexOf(id.source);
            return true;
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
