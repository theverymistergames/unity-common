using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MisterGames.Common.Strings;

namespace MisterGames.Common.Trees {

    public static class PathTree {

        public readonly struct Node<T> {

            public readonly T data;
            public readonly string name;

            public Node(string name, T data = default) {
                this.data = data;
                this.name = name;
            }

            public override string ToString() {
                return $"{nameof(Node<T>)}(name {name})";
            }
        }

        public static TreeEntry<Node<T>> CreateTree<T>(
            IEnumerable<T> collection,
            Func<T, string> getPath,
            char separator = '/',
            Func<IEnumerable<TreeEntry<Node<T>>>, IEnumerable<TreeEntry<Node<T>>>> sort = null
        ) {
            sort ??= SortChildren;
            var elements = collection.ToArray();

            var root = new TreeEntry<Node<T>> {
                children = GetChildren("", elements, getPath, 1, separator, sort)
            };

            return SquashParentsWithSingleChild(root, 0);
        }

        private static IEnumerable<TreeEntry<Node<T>>> SortChildren<T>(IEnumerable<TreeEntry<Node<T>>> children) {
            return children
                .OrderBy(e => e.children.Count == 0)
                .ThenBy(e => e.data.name);
        }

        private static List<TreeEntry<Node<T>>> GetChildren<T>(
            string parent,
            IReadOnlyList<T> elements,
            Func<T, string> getPath,
            int level,
            char separator,
            Func<IEnumerable<TreeEntry<Node<T>>>, IEnumerable<TreeEntry<Node<T>>>> sort
        ) {
            var sb = new StringBuilder();
            var nodes = new List<TreeEntry<Node<T>>>();
            var folderNameHashesSet = new HashSet<int>();

            for (int i = 0; i < elements.Count; i++) {
                var element = elements[i];
                string path = getPath.Invoke(element);

                if (!path.IsSubPathOf(parent, separator)) continue;

                string[] pathParts = path.Split(separator);
                int pathDepth = pathParts.Length;

                if (pathDepth <= level) {
                    string name = pathDepth == 0 ? string.Empty : pathParts[pathDepth - 1];

                    nodes.Add(new TreeEntry<Node<T>> {
                        data = new Node<T>(name, element),
                        level = level,
                        children = new List<TreeEntry<Node<T>>>(),
                    });
                    continue;
                }

                string folderName = pathParts[level - 1];
                int folderNameHash = folderName.GetHashCode();
                if (folderNameHashesSet.Contains(folderNameHash)) continue;

                sb.Clear();
                for (int p = 0; p < level; p++) {
                    sb.Append(pathParts[p]);
                    if (p < level - 1) sb.Append(separator);
                }
                string folderPath = sb.ToString();

                folderNameHashesSet.Add(folderNameHash);
                nodes.Add(new TreeEntry<Node<T>> {
                    data = new Node<T>(folderName),
                    level = level,
                    children = GetChildren(folderPath, elements, getPath, level + 1, separator, sort)
                });
            }

            return sort.Invoke(nodes).ToList();
        }

        private static TreeEntry<Node<T>> SquashParentsWithSingleChild<T>(TreeEntry<Node<T>> entry, int levelOffset) {
            entry.level += levelOffset;

            var children = entry.children;

            while (children.Count == 1) {
                var singleChild = children[0];

                if (singleChild.children.Count == 0) {
                    singleChild.level += levelOffset;
                    entry.children[0] = singleChild;

                    return entry;
                }

                entry.data = new Node<T>($"{entry.data.name}/{singleChild.data.name}", singleChild.data.data);
                entry.children = singleChild.children;

                levelOffset--;
                children = singleChild.children;
            }

            for (int i = 0; i < children.Count; i++) {
                children[i] = SquashParentsWithSingleChild(children[i], levelOffset);
            }

            return entry;
        }
    }

}
