﻿using MisterGames.Blueprints.Runtime;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Nodes;

namespace MisterGames.Blueprints {

    public static class BlueprintSources {

        public interface IEnter<TNode> : IBlueprintSource, IBlueprintEnter
            where TNode : struct, IBlueprintNode, IBlueprintEnter
        {
            void IBlueprintEnter.OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
                ref var node = ref GetNodeByRef<TNode>(token.node.node);
                node.OnEnterPort(blueprint, token, port);
            }
        }

        public interface IOutput<TNode, out R> : IBlueprintSource, IBlueprintOutput<R>
            where TNode : struct, IBlueprintNode, IBlueprintOutput<R>
        {
            R IBlueprintOutput<R>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
                ref var node = ref GetNodeByRef<TNode>(token.node.node);
                return node.GetPortValue(blueprint, token, port);
            }
        }

        public interface IOutput<TNode> : IBlueprintSource, IBlueprintOutput
            where TNode : struct, IBlueprintNode, IBlueprintOutput
        {
            R IBlueprintOutput.GetPortValue<R>(IBlueprint blueprint, NodeToken token, int port) {
                ref var node = ref GetNodeByRef<TNode>(token.node.node);
                return node.GetPortValue<R>(blueprint, token, port);
            }
        }

        public interface IStartCallback<TNode> : IBlueprintSource, IBlueprintStartCallback
            where TNode : struct, IBlueprintNode, IBlueprintStartCallback
        {
            void IBlueprintStartCallback.OnStart(IBlueprint blueprint, NodeToken token) {
                ref var node = ref GetNodeByRef<TNode>(token.node.node);
                node.OnStart(blueprint, token);
            }
        }

        public interface IEnableCallback<TNode> : IBlueprintSource, IBlueprintEnableCallback
            where TNode : struct, IBlueprintNode, IBlueprintEnableCallback
        {
            void IBlueprintEnableCallback.OnEnable(IBlueprint blueprint, NodeToken token, bool enabled) {
                ref var node = ref GetNodeByRef<TNode>(token.node.node);
                node.OnEnable(blueprint, token, enabled);
            }
        }

        public interface IConnectionCallback<TNode> : IBlueprintSource, IBlueprintConnectionCallback
            where TNode : struct, IBlueprintNode, IBlueprintConnectionCallback
        {
            void IBlueprintConnectionCallback.OnLinksChanged(IBlueprintMeta meta, NodeId id, int port) {
                ref var node = ref GetNodeByRef<TNode>(id.node);
                node.OnLinksChanged(meta, id, port);
            }
        }

        public interface ICloneable : IBlueprintSource { }

        public interface IInternalLink<TNode> : IBlueprintSource, IBlueprintInternalLink
            where TNode : struct, IBlueprintNode, IBlueprintInternalLink
        {
            void IBlueprintInternalLink.GetLinkedPorts(NodeId id, int port, out int index, out int count) {
                ref var node = ref GetNodeByRef<TNode>(id.node);
                node.GetLinkedPorts(id, port, out index, out count);
            }
        }

        public interface IHashLink<TNode> : IBlueprintSource, IBlueprintHashLink
            where TNode : struct, IBlueprintNode, IBlueprintHashLink
        {
            bool IBlueprintHashLink.TryGetLinkedPort(NodeId id, out int hash, out int port) {
                ref var node = ref GetNodeByRef<TNode>(id.node);
                return node.TryGetLinkedPort(id, out hash, out port);
            }
        }

        internal interface ICompilable<TNode> : IBlueprintSource, IBlueprintCompilable
            where TNode : struct, IBlueprintNode, IBlueprintCompilable
        {
            void IBlueprintCompilable.Compile(NodeId id, SubgraphCompileData data) {
                ref var node = ref GetNodeByRef<TNode>(id.node);
                node.Compile(id, data);
            }
        }

        internal interface ICreateSignaturePorts<TNode> : IBlueprintSource, IBlueprintCreateSignaturePorts
            where TNode : struct, IBlueprintNode, IBlueprintCreateSignaturePorts
        {
            bool IBlueprintCreateSignaturePorts.HasSignaturePorts(NodeId id) {
                ref var node = ref GetNodeByRef<TNode>(id.node);
                return node.HasSignaturePorts(id);
            }
        }
    }

}
