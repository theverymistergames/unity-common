using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Lists;
using MisterGames.Common.Trees;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace MisterGames.Common.Editor.Tree {

    public static class PathTree {

        public struct Node<T> {
            public T data;
            public string name;
            public int level;
            public bool isLeaf;
        }

        public static IEnumerable<Node<T>> CreateTree<T>(IEnumerable<T> collection, Func<T, string> getPath) {
            var elements = collection.ToList();
            return new TreeEntry<Node<T>> { children = GetChildren("", elements, getPath, 0) }
                .PreOrder()
                .RemoveRoot()
                .Select(e => e.data);
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
                        data = new Node<T> { data = element, name = name, level = level + 1, isLeaf = true },
                        level = level + 1,
                        children = new List<TreeEntry<Node<T>>>(),
                    });
                    continue;
                }

                string folderName = pathParts[level];
                if (folderNodes.Any(f => f.data.name == folderName)) continue;

                string folderPath = string.Join("/", pathParts.Slice(0, level));

                folderNodes.Add(new TreeEntry<Node<T>> {
                    data = new Node<T> { name = folderName, level = level + 1, isLeaf = false },
                    level = level + 1,
                    children = GetChildren(folderPath, elements, getPath, level + 1)
                });
            }

            var nodes = new List<TreeEntry<Node<T>>>();

            nodes.AddRange(folderNodes.OrderBy(e => e.data.name));
            nodes.AddRange(leafNodes.OrderBy(e => e.data.name));

            return nodes;
        }

        private static bool IsSubPathOf(this string sub, string parent) {
            int parentLength = parent.Length;
            if (parentLength > sub.Length) return false;
            return parent == sub[..parentLength];
        }
    }

}
