using System.Runtime.CompilerServices;

namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeBlueprint2 : IBlueprint {

        public IBlueprintHost2 Host { get; private set; }

        private readonly IBlueprintFactory _factory;
        private readonly IRuntimeNodeStorage _nodes;
        private readonly IRuntimeLinkStorage _links;

        private RuntimeBlueprint2() { }

        public RuntimeBlueprint2(IBlueprintFactory factory, IRuntimeNodeStorage nodes, IRuntimeLinkStorage links) {
            _factory = factory;
            _nodes = nodes;
            _links = links;
        }

        public void Initialize(IBlueprintHost2 host) {
            Host = host;

            for (int i = 0; i < _nodes.Count; i++) {
                var id = _nodes.GetNode(i);
                _factory.GetSource(id.source).OnInitialize(this, id);
            }
        }

        public void DeInitialize() {
            for (int i = 0; i < _nodes.Count; i++) {
                var id = _nodes.GetNode(i);
                _factory.GetSource(id.source).OnDeInitialize(this, id);
            }

            Host = null;
        }

        public void SetEnabled(bool enabled) {
            for (int i = 0; i < _nodes.Count; i++) {
                var id = _nodes.GetNode(i);

                if (_factory.GetSource(id.source) is IBlueprintEnableCallback enableDisable) {
                    enableDisable.OnEnable(this, id, enabled);
                }
            }
        }

        public void Start() {
            for (int i = 0; i < _nodes.Count; i++) {
                var id = _nodes.GetNode(i);

                if (_factory.GetSource(id.source) is IBlueprintStartCallback start) {
                    start.OnStart(this, id);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Call(NodeId id, int port) {
            for (int l = _links.GetFirstLink(id.source, id.node, port); l >= 0; l = _links.GetNextLink(l)) {
                var link = _links.GetLink(l);
                if (_factory.GetSource(link.source) is not IBlueprintEnter2 enter) continue;

                enter.OnEnterPort(this, new NodeId(link.source, link.node), link.port);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(NodeId id, int port, T defaultValue = default) {
            return ReadLink(_links.GetFirstLink(id.source, id.node, port), defaultValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadLink<T>(int index, T defaultValue = default) {
            if (index < 0) return defaultValue;

            var link = _links.GetLink(index);

            return _factory.GetSource(link.source) switch {
                IBlueprintOutput2<T> outputT => outputT.GetPortValue(this, new NodeId(link.source, link.node), link.port),
                IBlueprintOutput2 output => output.GetPortValue<T>(this, new NodeId(link.source, link.node), link.port),
                _ => defaultValue
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkReader GetLinks(NodeId id, int port) {
            return new LinkReader(this, _links.GetFirstLink(id.source, id.node, port));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNextLink(int previous) {
            return _links.GetNextLink(previous);
        }

        public override string ToString() {
            return $"{nameof(RuntimeBlueprint2)}: \nNodes: {_nodes}\nLinks: {_links}";
        }
    }

}
