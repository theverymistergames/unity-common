namespace MisterGames.Blueprints.Core2 {

    public static class BlueprintSources {

        public interface IEnter<TNode> : IBlueprintSource, IBlueprintEnter2
            where TNode : struct, IBlueprintNode, IBlueprintEnter2
        {
            void IBlueprintEnter2.OnEnterPort(IBlueprint blueprint, NodeId id, int port) {
                ref var node = ref GetNode<TNode>(id.node);
                node.OnEnterPort(blueprint, id, port);
            }
        }

        public interface IOutput<TNode, out R> : IBlueprintSource, IBlueprintOutput2<R>
            where TNode : struct, IBlueprintNode, IBlueprintOutput2<R>
        {
            R IBlueprintOutput2<R>.GetPortValue(IBlueprint blueprint, NodeId id, int port) {
                ref var node = ref GetNode<TNode>(id.node);
                return node.GetPortValue(blueprint, id, port);
            }
        }

        public interface IOutput<TNode> : IBlueprintSource, IBlueprintOutput2
            where TNode : struct, IBlueprintNode, IBlueprintOutput2
        {
            R IBlueprintOutput2.GetPortValue<R>(IBlueprint blueprint, NodeId id, int port) {
                ref var node = ref GetNode<TNode>(id.node);
                return node.GetPortValue<R>(blueprint, id, port);
            }
        }

        public interface IStartCallback<TNode> : IBlueprintSource, IBlueprintStartCallback
            where TNode : struct, IBlueprintNode, IBlueprintStartCallback
        {
            void IBlueprintStartCallback.OnStart(IBlueprint blueprint, NodeId id) {
                ref var node = ref GetNode<TNode>(id.node);
                node.OnStart(blueprint, id);
            }
        }

        public interface IEnableCallback<TNode> : IBlueprintSource, IBlueprintEnableCallback
            where TNode : struct, IBlueprintNode, IBlueprintEnableCallback
        {
            void IBlueprintEnableCallback.OnEnable(IBlueprint blueprint, NodeId id, bool enabled) {
                ref var node = ref GetNode<TNode>(id.node);
                node.OnEnable(blueprint, id, enabled);
            }
        }

        public interface IConnectionCallback<TNode> : IBlueprintSource, IBlueprintConnectionCallback
            where TNode : struct, IBlueprintNode, IBlueprintConnectionCallback
        {
            void IBlueprintConnectionCallback.OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
                ref var node = ref GetNode<TNode>(id.node);
                node.OnLinksChanged(meta, id, port);
            }
        }

        public interface IInternalLink<TNode> : IBlueprintSource, IBlueprintInternalLink
            where TNode : struct, IBlueprintNode, IBlueprintInternalLink
        {
            void IBlueprintInternalLink.GetLinkedPorts(NodeId id, int port, out int index, out int count) {
                ref var node = ref GetNode<TNode>(id.node);
                node.GetLinkedPorts(id, port, out index, out count);
            }
        }

        public interface IHashLink<TNode> : IBlueprintSource, IBlueprintHashLink
            where TNode : struct, IBlueprintNode, IBlueprintHashLink
        {
            void IBlueprintHashLink.GetLinkedPort(NodeId id, out int hash, out int port) {
                ref var node = ref GetNode<TNode>(id.node);
                node.GetLinkedPort(id, out hash, out port);
            }
        }

        internal interface ICompiled<TNode> : IBlueprintSource, IBlueprintCompiled
            where TNode : struct, IBlueprintNode, IBlueprintCompiled
        {
            void IBlueprintCompiled.Compile(IBlueprintFactory factory, BlueprintCompileData data) {
                ref var node = ref GetNode<TNode>(data.id.node);
                node.Compile(factory, data);
            }
        }
    }

}
