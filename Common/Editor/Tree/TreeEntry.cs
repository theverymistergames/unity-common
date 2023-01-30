using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MisterGames.Common.Editor.Tree {

    public struct TreeEntry<T> {

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
    }

}
