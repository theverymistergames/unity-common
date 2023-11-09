using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Nodes;

namespace MisterGames.Blueprints.Runtime {

    public sealed class RuntimeBlueprint2 : IBlueprint {

        public IBlueprintHost2 Host { get; private set; }
        public readonly NodeId root;

        internal readonly IBlueprintFactory factory;
        internal readonly IRuntimeNodeStorage nodeStorage;
        internal readonly IRuntimeLinkStorage linkStorage;
        internal readonly IRuntimeBlackboardStorage blackboardStorage;

        private readonly Dictionary<NodeId, ExternalBlueprintData> _externalBlueprintMap;

#if UNITY_EDITOR
        internal readonly Dictionary<int, Port> rootPorts = new Dictionary<int, Port>();
#endif

        private RuntimeBlueprint2() { }

        public RuntimeBlueprint2(
            NodeId root,
            IBlueprintFactory factory,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            IRuntimeBlackboardStorage blackboardStorage
        ) {
            this.root = root;

            this.factory = factory;
            this.nodeStorage = nodeStorage;
            this.linkStorage = linkStorage;
            this.blackboardStorage = blackboardStorage;

            _externalBlueprintMap = new Dictionary<NodeId, ExternalBlueprintData>();
        }

        public void Initialize(IBlueprintHost2 host) {
            Host = host;

            for (int i = 0; i < nodeStorage.Count; i++) {
                var id = nodeStorage.GetNode(i);
                factory.GetSource(id.source).OnInitialize(this, new NodeToken(id, root));
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < nodeStorage.Count; i++) {
                var id = nodeStorage.GetNode(i);
                var source = factory.GetSource(id.source);

                source.OnDeInitialize(this, new NodeToken(id, root));
                source.RemoveNode(id.node);
                if (source.Count == 0) factory.RemoveSource(id.source);
            }

            Host = null;
            blackboardStorage.Clear();
            _externalBlueprintMap.Clear();
        }

        public void SetEnabled(bool enabled) {
            var root = this.root;
            for (int i = 0; i < nodeStorage.Count; i++) {
                var id = nodeStorage.GetNode(i);

                if (factory.GetSource(id.source) is IBlueprintEnableCallback callback) {
                    callback.OnEnable(this, new NodeToken(id, root), enabled);
                }
            }
        }

        public void Start() {
            var root = this.root;
            for (int i = 0; i < nodeStorage.Count; i++) {
                var id = nodeStorage.GetNode(i);

                if (factory.GetSource(id.source) is IBlueprintStartCallback start) {
                    start.OnStart(this, new NodeToken(id, root));
                }
            }
        }

        public Blackboard GetBlackboard(NodeToken token) {
            return blackboardStorage.GetBlackboard(token.caller);
        }

        public void Call(NodeToken token, int port) {
            for (int l = GetFirstLink(token.node, port); l >= 0; l = GetNextLink(l)) {
                CallLink(l, token.caller);
            }
        }

        public T Read<T>(NodeToken token, int port, T defaultValue = default) {
            return ReadLink(GetFirstLink(token.node, port), token.caller, defaultValue);
        }

        public LinkIterator GetLinks(NodeToken token, int port) {
            return new LinkIterator(this, token, port);
        }

        internal void Bind(NodeId id, NodeId caller, IBlueprint blueprint) {
            _externalBlueprintMap[id] = new ExternalBlueprintData(caller, blueprint);
        }

        internal void Unbind(NodeId id) {
            _externalBlueprintMap?.Remove(id);
        }

        internal void ExternalCall(NodeId caller, int port) {
            if (!_externalBlueprintMap.TryGetValue(caller, out var data)) return;
            data.blueprint.Call(new NodeToken(caller, data.caller), port);
        }

        internal T ExternalRead<T>(NodeId caller, int port, T defaultValue = default) {
            return _externalBlueprintMap.TryGetValue(caller, out var data)
                ? data.blueprint.Read(new NodeToken(caller, data.caller), port, defaultValue)
                : defaultValue;
        }

        internal void CallLink(int index, NodeId caller) {
            if (index < 0) return;

            var link = linkStorage.GetLink(index);
            if (factory.GetSource(link.source) is not IBlueprintEnter2 enter) return;

            var token = new NodeToken(new NodeId(link.source, link.node), caller);
            enter.OnEnterPort(this, token, link.port);
        }

        internal T ReadLink<T>(int index, NodeId caller, T defaultValue = default) {
            if (index < 0) return defaultValue;

            var link = linkStorage.GetLink(index);
            var token = new NodeToken(new NodeId(link.source, link.node), caller);

            return factory.GetSource(link.source) switch {
                IBlueprintOutput2<T> outputT => outputT.GetPortValue(this, token, link.port),
                IBlueprintOutput2 output => output.GetPortValue<T>(this, token, link.port),
                _ => defaultValue
            };
        }

        internal int GetFirstLink(NodeId id, int port) {
            return linkStorage.GetFirstLink(id.source, id.node, port);
        }

        internal int GetNextLink(int previous) {
            return linkStorage.GetNextLink(previous);
        }

        public override string ToString() {
            return $"{nameof(RuntimeBlueprint2)}: Root: {root}\nNodes: {nodeStorage}\nLinks: {linkStorage}";
        }
    }

}
