using System;

namespace MisterGames.Blueprints.Runtime {

    public readonly struct RuntimeLink2 : IEquatable<RuntimeLink2> {

        public readonly int source;
        public readonly int node;
        public readonly int port;

        public RuntimeLink2(int source, int node, int port) {
            this.source = source;
            this.node = node;
            this.port = port;
        }

        public bool Equals(RuntimeLink2 other) {
            return source == other.source && node == other.node && port == other.port;
        }

        public override bool Equals(object obj) {
            return obj is RuntimeLink2 other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(source, node, port);
        }

        public static bool operator ==(RuntimeLink2 left, RuntimeLink2 right) {
            return left.Equals(right);
        }

        public static bool operator !=(RuntimeLink2 left, RuntimeLink2 right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(RuntimeLink2)}(source {source}, node {node}, port {port})";
        }
    }

}
