using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Runtime;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints {

    public sealed class BlueprintRunner2 : MonoBehaviour, IBlueprintHost2 {

        [SerializeField] private BlueprintAsset2 _blueprintAsset;
        [SerializeField] private TreeMap<NodeId, SubgraphData> _subgraphTree;
        [SerializeField] private Blackboard _blackboard;
        [SerializeReference] private BlueprintMeta2 _rootMetaOverride;
        [SerializeReference] private bool _isRootOverrideEnabled;

        public BlueprintAsset2 BlueprintAsset {
            get => _blueprintAsset;
            internal set => _blueprintAsset = value;
        }

        private RuntimeBlueprint2 _runtimeBlueprint;
        private BlueprintCompiler2 _compiler2;

        public RuntimeBlueprint2 GetOrCompileBlueprint() {
            if (_runtimeBlueprint != null) return _runtimeBlueprint;

            if (_blueprintAsset != null) {
                _runtimeBlueprint = _blueprintAsset.Compile(BlueprintFactories.Global, this);
            }
            else if (_isRootOverrideEnabled && _rootMetaOverride != null) {
                _compiler2 ??= new BlueprintCompiler2();
#if UNITY_EDITOR
                _rootMetaOverride.owner = this;
#endif
                _runtimeBlueprint = _compiler2.Compile(_rootMetaOverride, BlueprintFactories.Global, this);
            }

            _runtimeBlueprint?.Initialize(this);

            return _runtimeBlueprint;
        }

        private void Awake() {
            GetOrCompileBlueprint();
        }

        private void OnDestroy() {
            _runtimeBlueprint?.DeInitialize();
            _runtimeBlueprint = null;
        }

        private void OnEnable() {
            _runtimeBlueprint?.SetEnabled(true);
        }

        private void OnDisable() {
            _runtimeBlueprint?.SetEnabled(false);
        }

        private void Start() {
            _runtimeBlueprint?.Start();
        }

        public Blackboard GetRootBlackboard() {
            return _blackboard;
        }

        public IBlueprintFactory GetRootFactory() {
            return _isRootOverrideEnabled ? _rootMetaOverride?.Factory : null;
        }

        public int GetSubgraphIndex(NodeId id, int parent = -1) {
            return _subgraphTree.GetNode(id, parent);
        }

        public Blackboard GetSubgraphBlackboard(NodeId id, int parent = -1) {
            return _subgraphTree.TryGetValue(id, parent, out var data) ? data.blackboard : null;
        }

        public IBlueprintFactory GetSubgraphFactory(NodeId id, int parent = -1) {
            return _subgraphTree.TryGetValue(id, parent, out var data) && data.isFactoryOverrideEnabled ? data.factoryOverride : null;
        }

#if UNITY_EDITOR
        internal RuntimeBlueprint2 RuntimeBlueprint => _runtimeBlueprint;
        internal TreeMap<NodeId, SubgraphData> SubgraphTree => _subgraphTree ??= new TreeMap<NodeId, SubgraphData>();

        internal bool IsRootMetaOverrideEnabled {
            get => _isRootOverrideEnabled;
            set => _isRootOverrideEnabled = value;
        }

        internal BlueprintMeta2 RootMetaOverride {
            get {
                if (_rootMetaOverride != null) _rootMetaOverride.owner = this;
                return _rootMetaOverride;
            }
            set => _rootMetaOverride = value;
        }

        internal Blackboard RootBlackboard {
            get => _blackboard;
            set => _blackboard = value;
        }

        private readonly HashSet<MonoBehaviour> _clients = new HashSet<MonoBehaviour>();

        internal string GetNodePath(NodeId id, IBlueprintFactory factory) {
            if (factory == null ||
                !factory.TryGetNodePath(id, out int s, out int n)
            ) {
                return null;
            }

            if (_rootMetaOverride?.Factory == factory) {
                return $"_rootMetaOverride._factory._sources._entries.Array.data[{s}].value._nodeMap._entries.Array.data[{n}].value";
            }

            int i = _subgraphTree.FindNode(factory, (f, _, data) => f == data.factoryOverride);
            if (i < 0) return null;

            return $"_subgraphTree._nodes.Array.data[{i}].value.factoryOverride._sources._entries.Array.data[{s}].value._nodeMap._entries.Array.data[{n}].value";
        }

        internal NodeId[] FindSubgraphPath(BlueprintMeta2 meta) {
            if (meta == null || meta == _rootMetaOverride) return null;

            int n = _subgraphTree.FindNode(meta, (m, _, data) => m == data.asset.BlueprintMeta);
            if (n < 0) return null;

            int depth = _subgraphTree.GetNodeDepth(n);
            var path = new NodeId[depth + 1];

            for (int i = path.Length - 1; i >= 0; i--) {
                path[i] = _subgraphTree.GetKeyAt(n);
                n = _subgraphTree.GetParent(n);
            }

            return path;
        }

        internal bool TryFindSubgraph(NodeId[] path, out SubgraphData data) {
            if (path is not { Length: > 0 }) {
                data = default;
                return false;
            }

            int index = -1;

            for (int i = 0; i < path.Length; i++) {
                index = GetSubgraphIndex(path[i], index);
            }

            if (index < 0) {
                data = default;
                return false;
            }

            data = _subgraphTree.GetValueAt(index);
            return true;
        }

        internal void RestartBlueprint() {
            InterruptRuntimeBlueprint();
            RegisterClient(this);
            GetOrCompileBlueprint().Start();
        }

        internal void InterruptRuntimeBlueprint() {
            UnregisterClient(this);

            _runtimeBlueprint?.DeInitialize();
            _runtimeBlueprint = null;
        }

        internal void RegisterClient(MonoBehaviour client) {
            _clients.Add(client);
        }

        internal void UnregisterClient(MonoBehaviour client) {
            _clients.Remove(client);
            if (_clients.Count > 0) return;

            _runtimeBlueprint?.DeInitialize();
            _runtimeBlueprint = null;
        }
#endif
    }

}
