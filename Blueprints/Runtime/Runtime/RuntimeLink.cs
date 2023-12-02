using System;

namespace MisterGames.Blueprints.Runtime {

    public readonly struct RuntimeLink : IEquatable<RuntimeLink> {

        public readonly int source;
        public readonly int node;
        public readonly int port;

        public RuntimeLink(int source, int node, int port) {
            this.source = source;
            this.node = node;
            this.port = port;
        }

        public bool Equals(RuntimeLink other) {
            return source == other.source && node == other.node && port == other.port;
        }

        public override bool Equals(object obj) {
            return obj is RuntimeLink other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(source, node, port);
        }

        public static bool operator ==(RuntimeLink left, RuntimeLink right) {
            return left.Equals(right);
        }

        public static bool operator !=(RuntimeLink left, RuntimeLink right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"{nameof(RuntimeLink)}(source {source}, node {node}, port {port})";
        }
    }

}
