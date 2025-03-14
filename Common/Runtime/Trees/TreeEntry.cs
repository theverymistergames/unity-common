using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MisterGames.Common.Trees {

    public struct TreeEntry<T> : IEquatable<TreeEntry<T>> {

        public T data;
        public int level;
        public List<TreeEntry<T>> children;

        public override string ToString() {
            var tree = this.PreOrder().ToArray();
            var sb = new StringBuilder();

            for (int i = 0; i < tree.Length; i++) {
                var entry = tree[i];
                sb.AppendLine($"{new string('-', entry.level)}{nameof(TreeEntry<T>)} level {entry.level}, data {entry.data}, children:");
            }

            return sb.ToString();
        }

        public bool Equals(TreeEntry<T> other) {
            return EqualityComparer<T>.Default.Equals(data, other.data) && level == other.level && Equals(children, other.children);
        }

        public override bool Equals(object obj) {
            return obj is TreeEntry<T> other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(data, level, children);
        }

        public static bool operator ==(TreeEntry<T> left, TreeEntry<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(TreeEntry<T> left, TreeEntry<T> right) {
            return !left.Equals(right);
        }
    }

}
