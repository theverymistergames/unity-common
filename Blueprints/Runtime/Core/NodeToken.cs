﻿namespace MisterGames.Blueprints {

    public readonly struct NodeToken {

        public readonly NodeId node;
        public readonly NodeId caller;

        public NodeToken(NodeId node, NodeId caller) {
            this.node = node;
            this.caller = caller;
        }

        public override string ToString() {
            return $"{nameof(NodeToken)}(node {node}, caller {caller})";
        }
    }

}
