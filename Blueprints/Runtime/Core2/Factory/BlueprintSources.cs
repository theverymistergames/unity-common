namespace MisterGames.Blueprints.Core2 {

    public static class BlueprintSources {

        public interface IEnter<TNode> : IBlueprintSource, IBlueprintEnter2
            where TNode : struct, IBlueprintNode, IBlueprintEnter2
        {
            void IBlueprintEnter2.OnEnterPort(IBlueprint blueprint, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnEnterPort(blueprint, id, port);
            }
        }

        public interface IOutput<TNode, out R> : IBlueprintSource, IBlueprintOutput2<R>
            where TNode : struct, IBlueprintNode, IBlueprintOutput2<R>
        {
            R IBlueprintOutput2<R>.GetPortValue(IBlueprint blueprint, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                return node.GetPortValue(blueprint, id, port);
            }
        }

        public interface IOutput<TNode> : IBlueprintSource, IBlueprintOutput2
            where TNode : struct, IBlueprintNode, IBlueprintOutput2
        {
            R IBlueprintOutput2.GetOutputPortValue<R>(IBlueprint blueprint, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                return node.GetOutputPortValue<R>(blueprint, id, port);
            }
        }

        public interface IStartCallback<TNode> : IBlueprintSource, IBlueprintStartCallback
            where TNode : struct, IBlueprintNode, IBlueprintStartCallback
        {
            void IBlueprintStartCallback.OnStart(IBlueprint blueprint, long id) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnStart(blueprint, id);
            }
        }

        public interface IEnableCallback<TNode> : IBlueprintSource, IBlueprintEnableCallback
            where TNode : struct, IBlueprintNode, IBlueprintEnableCallback
        {
            void IBlueprintEnableCallback.OnEnable(IBlueprint blueprint, long id, bool enabled) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnEnable(blueprint, id, enabled);
            }
        }

        public interface IConnectionCallback<TNode> : IBlueprintSource, IBlueprintConnectionCallback
            where TNode : struct, IBlueprintNode, IBlueprintConnectionCallback
        {
            void IBlueprintConnectionCallback.OnLinksChanged(IBlueprintMeta meta, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnLinksChanged(meta, id, port);
            }
        }

        internal interface IPortLinker<TNode> : IBlueprintSource, IBlueprintPortLinker2
            where TNode : struct, IBlueprintNode, IBlueprintPortLinker2
        {
            void IBlueprintPortLinker2.GetLinkedPorts(long id, int port, out int index, out int count) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.GetLinkedPorts(id, port, out index, out count);
            }
        }

        internal interface INodeLinker<TNode> : IBlueprintSource, IBlueprintNodeLinker2
            where TNode : struct, IBlueprintNode, IBlueprintNodeLinker2
        {
            void IBlueprintNodeLinker2.GetLinkedNode(long id, out int hash, out int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.GetLinkedNode(id, out hash, out port);
            }
        }

        internal interface ICompilationCallback<TNode> : IBlueprintSource, IBlueprintCompilationCallback
            where TNode : struct, IBlueprintNode, IBlueprintCompilationCallback
        {
            void IBlueprintCompilationCallback.OnCompile(long id, Port[] ports) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnCompile(id, ports);
            }
        }
    }

}
