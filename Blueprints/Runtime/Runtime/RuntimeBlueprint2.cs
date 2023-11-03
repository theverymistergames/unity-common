using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Nodes;

namespace MisterGames.Blueprints.Runtime {

    public sealed class RuntimeBlueprint2 : IBlueprint {

        public NodeId Root => _linkStorage.Root;
        public IBlueprintHost2 Host { get; private set; }
        public Blackboard Blackboard { get; private set; }

        private readonly IBlueprintFactory _factory;
        private readonly IRuntimeNodeStorage _nodeStorage;
        private readonly IRuntimeLinkStorage _linkStorage;
        private Dictionary<NodeId, ExternalBlueprintData> _rootMap;

        private RuntimeBlueprint2() { }

        public RuntimeBlueprint2(
            IBlueprintFactory factory,
            IRuntimeNodeStorage nodeStorage,
            IRuntimeLinkStorage linkStorage
        ) {
            _factory = factory;
            _nodeStorage = nodeStorage;
            _linkStorage = linkStorage;
        }

        public void Initialize(IBlueprintHost2 host, Blackboard blackboard) {
            Host = host;
            Blackboard = blackboard;

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
            Blackboard = null;
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

        public void Bind(NodeId id, NodeId caller, IBlueprint blueprint) {
            _rootMap ??= new Dictionary<NodeId, ExternalBlueprintData>();
            _rootMap[id] = new ExternalBlueprintData(caller, blueprint);
        }

        public void Unbind(NodeId id) {
            _rootMap?.Remove(id);
        }

        public void CallRoot(NodeId caller, int port) {
            var root = _linkStorage.Root;
            for (int l = GetFirstLink(root, port); l >= 0; l = _linkStorage.GetNextLink(l)) {
                CallLink(l, caller);
            }
        }

        public T ReadRoot<T>(NodeId caller, int port, T defaultValue = default) {
            var root = _linkStorage.Root;
            return ReadLink(GetFirstLink(root, port), caller, defaultValue);
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

        public int GetFirstLink(NodeId id, int port) {
            return _linkStorage.GetFirstLink(id.source, id.node, port);
        }

        public int GetNextLink(int previous) {
            return _linkStorage.GetNextLink(previous);
        }

        public void CallLink(int index, NodeId caller) {
            if (index < 0) return;

            var link = _linkStorage.GetLink(index);
            if (_factory.GetSource(link.source) is not IBlueprintEnter2 enter) return;

            var token = new NodeToken(new NodeId(link.source, link.node), caller);
            enter.OnEnterPort(this, token, link.port);
        }

        public T ReadLink<T>(int index, NodeId caller, T defaultValue = default) {
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
