using System;

namespace MisterGames.Blueprints {

    [Serializable]
    public struct NodeId : IEquatable<NodeId> {

        public int source;
        public int node;

        public NodeId(int source, int node) {
            this.source = source;
            this.node = node;
        }

        public bool Equals(NodeId other) {
            return source == other.source && node == other.node;
        }

        public override bool Equals(object obj) {
            return obj is NodeId n && Equals(n);
        }

        public override int GetHashCode() {
            return HashCode.Combine(source, node);
        }

        public static bool operator ==(NodeId left, NodeId right) {
            return left.Equals(right);
        }

        public static bool operator !=(NodeId left, NodeId right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(NodeId)}({source}.{node})";
        }
    }

}
