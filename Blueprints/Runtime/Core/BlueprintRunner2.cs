using System.Collections.Generic;
using System.Text;
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
        [SerializeReference] private BlueprintMeta2 _rootOverrideMeta;

        public BlueprintAsset2 BlueprintAsset => _blueprintAsset;
        public MonoBehaviour Runner => this;

        private RuntimeBlueprint2 _runtimeBlueprint;
        private BlueprintCompiler2 _compiler2;

        public RuntimeBlueprint2 GetOrCompileBlueprint() {
            if (_runtimeBlueprint != null) return _runtimeBlueprint;

            if (_blueprintAsset != null) {
                _runtimeBlueprint = _blueprintAsset.Compile(BlueprintFactories.Global, this);
            }
            else {
                _compiler2 ??= new BlueprintCompiler2();
                _runtimeBlueprint = _compiler2.Compile(BlueprintFactories.Global, this);
            }

            _runtimeBlueprint.Initialize(this);

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

        public int GetSubgraphIndex(NodeId id, int parent = -1) {
            return _subgraphTree.GetNode(id, parent);
        }

        public Blackboard GetBlackboard(NodeId id = default, int parent = -1) {
            if (id == default) return _blackboard;
            return _subgraphTree.TryGetValue(id, parent, out var data) ? data.blackboard : null;
        }

        public BlueprintMeta2 GetBlueprintMeta(NodeId id = default, int parent = -1) {
            if (id == default) {
                var meta = _blueprintAsset == null ? null : _blueprintAsset.BlueprintMeta;

                if (_rootOverrideMeta != null) {
                    _rootOverrideMeta.owner = this;
                    _rootOverrideMeta.overridenMeta = meta;
                    meta = _rootOverrideMeta;
                }

#if UNITY_EDITOR
                meta?.NodeJsonMap.Clear();
#endif

                return meta;
            }

            if (_subgraphTree.TryGetValue(id, parent, out var data)) {
                var meta = data.asset.BlueprintMeta;

                if (data.metaOverride != null) {
                    data.metaOverride.owner = this;
                    data.metaOverride.overridenMeta = meta;
                    meta = data.metaOverride;
                }

#if UNITY_EDITOR
                meta?.NodeJsonMap.Clear();
#endif

                return meta;
            }

            return null;
        }

#if UNITY_EDITOR
        internal RuntimeBlueprint2 RuntimeBlueprint => _runtimeBlueprint;
        internal TreeMap<NodeId, SubgraphData> SubgraphTree =>
            _subgraphTree ??= new TreeMap<NodeId, SubgraphData>();

        private readonly HashSet<MonoBehaviour> _clients = new HashSet<MonoBehaviour>();

        internal string GetNodePath(BlueprintMeta2 meta, NodeId id) {
            if (!meta.TryGetNodePath(id, out int si, out int ni)) return null;

            int n = _subgraphTree.FindNode(meta, (m, _, data) => m == data.metaOverride);

            if (n < 0) {
                return meta == _rootOverrideMeta
                    ? $"_rootOverrideMeta._factory._sources._entries.Array.data[{si}].value._nodeMap._entries.Array.data[{ni}].value"
                    : null;
            }

            return $"_subgraphTree._nodes.Array.data[{n}].value.metaOverride._factory._sources._entries.Array.data[{si}].value._nodeMap._entries.Array.data[{ni}].value";
        }

        internal NodeId[] FindSubgraphPath(BlueprintMeta2 meta) {
            int n = _subgraphTree.FindNode(meta, (m, _, data) => m == data.metaOverride);
            if (n < 0) return null;

            int depth = _subgraphTree.GetNodeDepth(n);
            var path = new NodeId[depth + 1];

            for (int i = path.Length - 1; i >= 0; i--) {
                path[i] = _subgraphTree.GetKeyAt(n);
                n = _subgraphTree.GetParent(n);
            }

            return path;
        }

        internal bool TryFindSubgraph(NodeId[] path, out BlueprintMeta2 meta, out Blackboard blackboard) {
            meta = null;
            blackboard = null;

            int index = -1;

            var sb = new StringBuilder();

            for (int i = 0; i < path.Length; i++) {
                int parent = index;
                var nodeId = path[i];
                index = GetSubgraphIndex(nodeId, parent);

                sb.Append($"[{nodeId.source}.{nodeId.node}] =>");
            }

            if (index < 0) return false;

            ref var data = ref _subgraphTree.GetValueAt(index);

            meta = data.metaOverride;
            blackboard = data.asset.Blackboard;

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
