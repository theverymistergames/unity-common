namespace MisterGames.Blueprints.Core2 {

    public static class BlueprintFactories {

        public interface Enter<TNode> : IBlueprintFactory, IBlueprintEnter2
            where TNode : struct, IBlueprintNode, IBlueprintEnter2
        {
            void IBlueprintEnter2.OnEnterPort(IBlueprint blueprint, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnEnterPort(blueprint, id, port);
            }
        }

        public interface Output<TNode, out R> : IBlueprintFactory, IBlueprintOutput2<R>
            where TNode : struct, IBlueprintNode, IBlueprintOutput2<R>
        {
            R IBlueprintOutput2<R>.GetOutputPortValue(IBlueprint blueprint, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                return node.GetOutputPortValue(blueprint, id, port);
            }
        }

        public interface DynamicOutput<TNode> : IBlueprintFactory, IBlueprintOutput2
            where TNode : struct, IBlueprintNode, IBlueprintOutput2
        {
            R IBlueprintOutput2.GetOutputPortValue<R>(IBlueprint blueprint, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                return node.GetOutputPortValue<R>(blueprint, id, port);
            }
        }

        public interface Start<TNode> : IBlueprintFactory, IBlueprintStart2
            where TNode : struct, IBlueprintNode, IBlueprintStart2
        {
            void IBlueprintStart2.OnStart(IBlueprint blueprint, long id) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnStart(blueprint, id);
            }
        }

        public interface EnableDisable<TNode> : IBlueprintFactory, IBlueprintEnableDisable2
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

        public interface ConnectionsCallback<TNode> : IBlueprintFactory, IBlueprintConnectionsCallback
            where TNode : struct, IBlueprintNode, IBlueprintConnectionsCallback
        {
            void IBlueprintConnectionsCallback.OnConnectionsChanged(IBlueprintMeta meta, long id, int port) {
                BlueprintNodeAddress.Unpack(id, out _, out int nodeId);

                ref var node = ref GetNode<TNode>(nodeId);
                node.OnConnectionsChanged(meta, id, port);
            }
        }
    }

}
