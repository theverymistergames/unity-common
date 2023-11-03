using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Nodes;

namespace MisterGames.Blueprints.Runtime {

    public sealed class RuntimeBlueprint2 : IBlueprint {

        public IBlueprintHost2 Host { get; private set; }

        private readonly IBlueprintFactory _factory;
        private readonly IRuntimeNodeStorage _nodeStorage;
        private readonly IRuntimeLinkStorage _linkStorage;
        private readonly IRuntimeBlackboardStorage _blackboardStorage;

        private readonly Dictionary<NodeId, ExternalBlueprintData> _rootMap = new Dictionary<NodeId, ExternalBlueprintData>();

        private RuntimeBlueprint2() { }

        public RuntimeBlueprint2(
            IBlueprintFactory factory,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage,
            IRuntimeBlackboardStorage blackboardStorage
        ) {
            _factory = factory;
            _nodeStorage = nodeStorage;
            _linkStorage = linkStorage;
            _blackboardStorage = blackboardStorage;
        }

        public void Initialize(IBlueprintHost2 host) {
            Host = host;
            var root = _linkStorage.Root;

            for (int i = 0; i < _nodeStorage.Count; i++) {
                var id = _nodeStorage.GetNode(i);
                _factory.GetSource(id.source).OnInitialize(this, new NodeToken(id, root));
            }
        }

        public void DeInitialize() {
            var root = _linkStorage.Root;

            for (int i = 0; i < _nodeStorage.Count; i++) {
                var id = _nodeStorage.GetNode(i);
                var source = _factory.GetSource(id.source);

                source.OnDeInitialize(this, new NodeToken(id, root));
                source.RemoveNode(id.node);
                if (source.Count == 0) _factory.RemoveSource(id.source);
            }

            Host = null;
            _rootMap.Clear();
            _blackboardStorage.Clear();
        }

        public void SetEnabled(bool enabled) {
            var root = _linkStorage.Root;
            for (int i = 0; i < _nodeStorage.Count; i++) {
                var id = _nodeStorage.GetNode(i);

                if (_factory.GetSource(id.source) is IBlueprintEnableCallback callback) {
                    callback.OnEnable(this, new NodeToken(id, root), enabled);
                }
            }
        }

        public void Start() {
            var root = _linkStorage.Root;
            for (int i = 0; i < _nodeStorage.Count; i++) {
                var id = _nodeStorage.GetNode(i);

                if (_factory.GetSource(id.source) is IBlueprintStartCallback start) {
                    start.OnStart(this, new NodeToken(id, root));
                }
            }
        }

        public Blackboard GetBlackboard(NodeToken token) {
            return _blackboardStorage.GetBlackboard(token.caller);
        }

        public void Call(NodeToken token, int port) {
            if (token.node == _linkStorage.Root) {
                var data = _rootMap[token.caller];
                data.blueprint.Call(new NodeToken(token.caller, data.caller), port);
                return;
            }

            for (int l = GetFirstLink(token.node, port); l >= 0; l = _linkStorage.GetNextLink(l)) {
                CallLink(l, token.caller);
            }
        }

        public T Read<T>(NodeToken token, int port, T defaultValue = default) {
            if (token.node == _linkStorage.Root) {
                var data = _rootMap[token.caller];
                return data.blueprint.Read<T>(new NodeToken(token.caller, data.caller), port);
            }

            return ReadLink(GetFirstLink(token.node, port), token.caller, defaultValue);
        }

        public LinkIterator GetLinks(NodeToken token, int port) {
            return new LinkIterator(this, token, port);
        }

        internal void Bind(NodeId id, NodeId caller, IBlueprint blueprint) {
            _rootMap[id] = new ExternalBlueprintData(caller, blueprint);
        }

        internal void Unbind(NodeId id) {
            _rootMap?.Remove(id);
        }

        internal void CallRoot(NodeId caller, int port) {
            for (int l = GetFirstLink(_linkStorage.Root, port); l >= 0; l = GetNextLink(l)) {
                CallLink(l, caller);
            }
        }

        internal T ReadRoot<T>(NodeId caller, int port, T defaultValue = default) {
            return ReadLink(GetFirstLink(_linkStorage.Root, port), caller, defaultValue);
        }

        internal int GetFirstLink(NodeId id, int port) {
            return _linkStorage.GetFirstLink(id.source, id.node, port);
        }

        internal int GetNextLink(int previous) {
            return _linkStorage.GetNextLink(previous);
        }

        internal void CallLink(int index, NodeId caller) {
            if (index < 0) return;

            var link = _linkStorage.GetLink(index);
            if (_factory.GetSource(link.source) is not IBlueprintEnter2 enter) return;

            var token = new NodeToken(new NodeId(link.source, link.node), caller);
            enter.OnEnterPort(this, token, link.port);
        }

        internal T ReadLink<T>(int index, NodeId caller, T defaultValue = default) {
            if (index < 0) return defaultValue;

            var link = _linkStorage.GetLink(index);
            var token = new NodeToken(new NodeId(link.source, link.node), caller);

            return _factory.GetSource(link.source) switch {
                IBlueprintOutput2<T> outputT => outputT.GetPortValue(this, token, link.port),
                IBlueprintOutput2 output => output.GetPortValue<T>(this, token, link.port),
                _ => defaultValue
            };
        }

        public override string ToString() {
            return $"{nameof(RuntimeBlueprint2)}: \nNodes: {_nodeStorage}\nLinks: {_linkStorage}";
        }
    }

}
