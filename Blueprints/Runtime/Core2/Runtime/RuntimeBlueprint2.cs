using System.Runtime.CompilerServices;

namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeBlueprint2 : IBlueprint {

        public NodeId Root => _linkStorage.Root;
        public IBlueprintHost2 Host { get; private set; }

        private readonly IBlueprintFactory _factory;
        private readonly IRuntimeNodeStorage _nodeStorage;
        private readonly IRuntimeLinkStorage _linkStorage;

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

        public void Initialize(IBlueprintHost2 host) {
            Host = host;

            for (int i = 0; i < _nodeStorage.Count; i++) {
                var id = _nodeStorage.GetNode(i);
                _factory.GetSource(id.source).OnInitialize(this, id);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _nodeStorage.Count; i++) {
                var id = _nodeStorage.GetNode(i);
                _factory.GetSource(id.source).OnDeInitialize(this, id);
            }

            Host = null;
        }

        public void Destroy() {
            for (int i = 0; i < _nodeStorage.Count; i++) {
                var id = _nodeStorage.GetNode(i);
                var source = _factory.GetSource(id.source);

                source.RemoveNode(id.node);
                if (source.Count == 0) _factory.RemoveSource(id.source);
            }
        }

        public void SetEnabled(bool enabled) {
            for (int i = 0; i < _nodeStorage.Count; i++) {
                var id = _nodeStorage.GetNode(i);

                if (_factory.GetSource(id.source) is IBlueprintEnableCallback callback) {
                    callback.OnEnable(this, id, enabled);
                }
            }
        }

        public void Start() {
            for (int i = 0; i < _nodeStorage.Count; i++) {
                var id = _nodeStorage.GetNode(i);

                if (_factory.GetSource(id.source) is IBlueprintStartCallback start) {
                    start.OnStart(this, id);
                }
            }
        }

        public void Bind(NodeId id) {
            var root = _linkStorage.Root;
            var rootPorts = _linkStorage.RootPorts;

            foreach (int rootPort in rootPorts) {
                int i = _linkStorage.SelectPort(root.source, root.node, rootPort);
                _linkStorage.InsertLinkAfter(i, id.source, id.node, rootPort);
            }
        }

        public void Unbind(NodeId id) {
            var root = _linkStorage.Root;
            var rootPorts = _linkStorage.RootPorts;

            foreach (int rootPort in rootPorts) {
                _linkStorage.SelectPort(root.source, root.node, rootPort);
                _linkStorage.RemoveLink(id.source, id.node, rootPort);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Call(NodeId id, int port) {
            for (int l = _linkStorage.GetFirstLink(id.source, id.node, port); l >= 0; l = _linkStorage.GetNextLink(l)) {
                var link = _linkStorage.GetLink(l);
                if (_factory.GetSource(link.source) is not IBlueprintEnter2 enter) continue;

                enter.OnEnterPort(this, new NodeId(link.source, link.node), link.port);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(NodeId id, int port, T defaultValue = default) {
            return ReadLink(_linkStorage.GetFirstLink(id.source, id.node, port), defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadLink<T>(int index, T defaultValue = default) {
            if (index < 0) return defaultValue;

            var link = _linkStorage.GetLink(index);

            return _factory.GetSource(link.source) switch {
                IBlueprintOutput2<T> outputT => outputT.GetPortValue(this, new NodeId(link.source, link.node), link.port),
                IBlueprintOutput2 output => output.GetPortValue<T>(this, new NodeId(link.source, link.node), link.port),
                _ => defaultValue
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkReader GetLinks(NodeId id, int port) {
            return new LinkReader(this, _linkStorage, id, port);
        }

        public override string ToString() {
            return $"{nameof(RuntimeBlueprint2)}: \nNodes: {_nodeStorage}\nLinks: {_linkStorage}";
        }
    }

}
