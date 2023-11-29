using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Factory {

    [Serializable]
    public sealed class BlueprintFactory : IBlueprintFactory {

        [SerializeField] private ReferenceMap<int, IBlueprintSource> _sources;
        [SerializeField] private int _lastId;

        private readonly Dictionary<Type, int> _typeToIdMap = new Dictionary<Type, int>();

        public BlueprintFactory() {
            _sources = new ReferenceMap<int, IBlueprintSource>();
        }

        public BlueprintFactory(BlueprintFactory source) {
            _sources = new ReferenceMap<int, IBlueprintSource>(source._sources);
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

        public IBlueprintSource GetOrCreateSource(int id, Type sourceType) {
            if (_sources.TryGetValue(id, out var source)) return source;

            source = Activator.CreateInstance(sourceType) as IBlueprintSource;
            _sources.Add(id, source);

            return source;
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

        public bool MatchNodesWith(IBlueprintFactory factory) {
            bool changed = false;

            foreach (int id in _sources.Keys) {
                if (factory.GetSource(id) is {} s) changed = _sources[id].MatchNodesWith(s);
            }

            changed |= _sources.RemoveIf(factory, (f, id) => f.GetSource(id) == null);

            if (changed) _typeToIdMap.Clear();

            return changed;
        }

        public void AdditiveCopyInto(IBlueprintFactory factory) {
            if (factory is not BlueprintFactory f) return;

            foreach ((int id, var source) in _sources) {
                if (!f._sources.TryGetValue(id, out var s)) {
                    s = Activator.CreateInstance(source.GetType()) as IBlueprintSource;
                    f._sources.Add(id, s);
                }

                if (s != null) source.AdditiveCopyInto(s);
            }

            if (f._lastId < _lastId) f._lastId = _lastId;
        }

        private int AddSource(IBlueprintSource source) {
            _lastId++;
            if (_lastId == 0) _lastId++;

            int id = _lastId;
            _sources.Add(id, source);

            return id;
        }

        public override string ToString() {
            return $"{nameof(BlueprintFactory)}: sources {_sources}";
        }
    }

}
