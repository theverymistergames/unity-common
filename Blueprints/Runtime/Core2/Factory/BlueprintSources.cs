namespace MisterGames.Blueprints.Core2 {

    public static class BlueprintSources {

        public interface Enter<TNode> : IBlueprintSource, IBlueprintEnter2
            where TNode : struct, IBlueprintNode, IBlueprintEnter2
        {
            void IBlueprintEnter2.OnEnterPort(IBlueprint blueprint, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnEnterPort(blueprint, id, port);
            }
        }

        public interface Output<TNode, out R> : IBlueprintSource, IBlueprintOutput2<R>
            where TNode : struct, IBlueprintNode, IBlueprintOutput2<R>
        {
            R IBlueprintOutput2<R>.GetOutputPortValue(IBlueprint blueprint, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                return node.GetOutputPortValue(blueprint, id, port);
            }
        }

        public interface DynamicOutput<TNode> : IBlueprintSource, IBlueprintOutput2
            where TNode : struct, IBlueprintNode, IBlueprintOutput2
        {
            R IBlueprintOutput2.GetOutputPortValue<R>(IBlueprint blueprint, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                return node.GetOutputPortValue<R>(blueprint, id, port);
            }
        }

        public interface Start<TNode> : IBlueprintSource, IBlueprintStart2
            where TNode : struct, IBlueprintNode, IBlueprintStart2
        {
            void IBlueprintStart2.OnStart(IBlueprint blueprint, long id) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnStart(blueprint, id);
            }
        }

        public interface EnableDisable<TNode> : IBlueprintSource, IBlueprintEnableDisable2
            where TNode : struct, IBlueprintNode, IBlueprintEnableDisable2
        {
            void IBlueprintEnableDisable2.OnEnable(IBlueprint blueprint, long id) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnEnable(blueprint, id);
            }

            void IBlueprintEnableDisable2.OnDisable(IBlueprint blueprint, long id) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnDisable(blueprint, id);
            }
        }

        public interface ConnectionsCallback<TNode> : IBlueprintSource, IBlueprintConnectionsCallback
            where TNode : struct, IBlueprintNode, IBlueprintConnectionsCallback
        {
            void IBlueprintConnectionsCallback.OnLinksChanged(IBlueprintMeta meta, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnLinksChanged(meta, id, port);
            }
        }

        internal interface PortLinker<TNode> : IBlueprintSource, IBlueprintPortLinker2
            where TNode : struct, IBlueprintNode, IBlueprintPortLinker2
        {
            void IBlueprintPortLinker2.GetLinkedPorts(long id, int port, out int index, out int count) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.GetLinkedPorts(id, port, out index, out count);
            }
        }

        internal interface NodeLinker<TNode> : IBlueprintSource, IBlueprintNodeLinker2
            where TNode : struct, IBlueprintNode, IBlueprintNodeLinker2
        {
            void IBlueprintNodeLinker2.GetLinkedNode(long id, out int hash, out int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.GetLinkedNode(id, out hash, out port);
            }
        }

        internal interface Compiled<TNode> : IBlueprintSource, IBlueprintCompiled
            where TNode : struct, IBlueprintNode, IBlueprintCompiled
        {
            void IBlueprintCompiled.Compile(long id, Port[] ports) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.Compile(id, ports);
            }
        }
    }

}
