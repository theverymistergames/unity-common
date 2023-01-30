using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Common.Editor.Tree {

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

        public static TreeEntry<Node<T>> CreateTree<T>(IEnumerable<T> collection, Func<T, string> getPath) {
            var elements = collection.ToArray();
            var root = new TreeEntry<Node<T>> {
                level = 0,
                children = GetChildren("", elements, getPath, 0)
            };

            return root.SquashParentsWithSingleChild();
        }

        private static List<TreeEntry<Node<T>>> GetChildren<T>(
            string parent,
            IReadOnlyList<T> elements,
            Func<T, string> getPath,
            int level
        ) {
            var leafNodes = new List<TreeEntry<Node<T>>>();
            var folderNodes = new List<TreeEntry<Node<T>>>();

            for (int i = elements.Count - 1; i >= 0; i--) {
                var element = elements[i];

                string path = getPath.Invoke(element);
                if (!path.IsSubPathOf(parent)) continue;

                string[] pathParts = path.Split('/');
                int pathDepth = pathParts.Length;

                if (pathDepth <= level + 1) {
                    string name = pathDepth == 0 ? string.Empty : pathParts[pathDepth - 1];
                    leafNodes.Add(new TreeEntry<Node<T>> {
                        data = new Node<T>(name, element),
                        level = level + 1,
                        children = new List<TreeEntry<Node<T>>>(),
                    });
                    continue;
                }

                string folderName = pathParts[level];
                if (folderNodes.Any(f => f.data.name == folderName)) continue;

                string folderPath = string.Join("/", pathParts.Slice(0, level));

                folderNodes.Add(new TreeEntry<Node<T>> {
                    data = new Node<T>(folderName),
                    level = level + 1,
                    children = GetChildren(folderPath, elements, getPath, level + 1)
                });
            }

            var nodes = new List<TreeEntry<Node<T>>>();

            nodes.AddRange(folderNodes.OrderBy(e => e.data.name));
            nodes.AddRange(leafNodes.OrderBy(e => e.data.name));

            return nodes;
        }

        private static TreeEntry<Node<T>> SquashParentsWithSingleChild<T>(this TreeEntry<Node<T>> entry, int levelOffset = 0) {
            entry.level += levelOffset;

            var children = entry.children;

            while (children.Count == 1) {
                var singleChild = children[0];

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

        private static bool IsSubPathOf(this string sub, string parent) {
            int parentLength = parent.Length;
            if (parentLength > sub.Length) return false;
            return parent == sub[..parentLength];
        }
    }

}
