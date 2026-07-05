using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MisterGames.Blackboards.Tables;
using MisterGames.Common.Trees;
using MisterGames.Common.Types;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Editor {

    internal static class BlackboardTypeCache {

        private const string Editor = "editor";
        private static readonly ManualResetEventSlim _treeNodesReadyEvent = new(false);
        private static readonly object _searchTreeLock = new();
        private static bool _buildStarted;
        private static List<TreeNode> _treeNodes;
        private static List<SearchTreeEntry> _searchTree;

        public static List<SearchTreeEntry> SearchTree {
            get {
                EnsureBuildStarted();
                _treeNodesReadyEvent.Wait();

                if (_searchTree != null) return _searchTree;

                lock (_searchTreeLock) {
                    _searchTree ??= CreateSearchTree(_treeNodes);
                }

                return _searchTree;
            }
        }

        private static void EnsureBuildStarted() {
            if (_buildStarted) return;

            lock (_searchTreeLock) {
                if (_buildStarted) return;
                _buildStarted = true;

                var thread = new Thread(BuildTreeNodesInBackground) {
                    IsBackground = true,
                    Name = nameof(BlackboardTypeCache) + ".BuildSearchTree",
                };
                thread.Start();
            }
        }

        private static void BuildTreeNodesInBackground() {
            _treeNodes = CreateTypeTree();
            _treeNodesReadyEvent.Set();
        }

        private readonly struct TreeNode {
            public readonly string title;
            public readonly Type type;
            public readonly int level;
            public readonly bool isGroup;

            public TreeNode(string title, int level) {
                this.title = title;
                this.level = level;
                type = null;
                isGroup = true;
            }

            public TreeNode(string title, Type type, int level) {
                this.title = title;
                this.type = type;
                this.level = level;
                isGroup = false;
            }
        }

        private static List<TreeNode> CreateTypeTree() {
            var tree = new List<TreeNode>();

            var assemblyTypes = CollectAssemblyTypes();

            var types = assemblyTypes
                .Where(t => t.IsEnum && BlackboardTableUtils.IsSupportedElementType(t))
                .ToArray();
            tree.AddRange(GetTypeTree("Enums", types, 1));

            types = assemblyTypes
                .Where(t => t.IsInterface && BlackboardTableUtils.IsSupportedElementType(t))
                .ToArray();
            tree.AddRange(GetTypeTree("Interfaces", types, 1));

            types = assemblyTypes
                .Where(t => t.IsClass && !typeof(Object).IsAssignableFrom(t) && BlackboardTableUtils.IsSupportedElementType(t))
                .ToArray();
            tree.AddRange(GetTypeTree("System.Object", types, 1));

            types = assemblyTypes
                .Where(t => t.IsClass && typeof(Object).IsAssignableFrom(t) && BlackboardTableUtils.IsSupportedElementType(t))
                .ToArray();
            tree.AddRange(GetTypeTree("UnityEngine.Object", types, 1));

            types = assemblyTypes
                .Where(t => t.IsValueType && !t.IsEnum && BlackboardTableUtils.IsSupportedElementType(t))
                .ToArray();
            tree.AddRange(GetTypeTree("Value types", types, 1));

            return tree;
        }

        private static IEnumerable<TreeNode> GetTypeTree(string rootName, IReadOnlyList<Type> types, int level) {
            if (types.Count == 0) return Array.Empty<TreeNode>();

            if (types.Count == 1) {
                var type = types[0];
                return new List<TreeNode> {
                    new(TypeNameFormatter.GetFullTypeNamePathInBraces(type), type, level)
                };
            }

            var tree = new List<TreeNode> { new(rootName, level) };

            var pathTree = PathTree
                .CreateTree(types, t => t.FullName, '.')
                .PreOrder()
                .Where(e => !string.IsNullOrEmpty(e.data.name))
                .Select(e => ToTreeNode(e, level));

            tree.AddRange(pathTree);

            return tree;
        }

        private static TreeNode ToTreeNode(TreeEntry<PathTree.Node<Type>> treeEntry, int levelOffset) {
            if (treeEntry.children.Count == 0) {
                var type = treeEntry.data.data;
                return new TreeNode(
                    TypeNameFormatter.GetFullTypeNamePathInBraces(type),
                    type,
                    treeEntry.level + levelOffset
                );
            }

            return new TreeNode(treeEntry.data.name, treeEntry.level + levelOffset);
        }

        private static Type[] CollectAssemblyTypes() {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.FullName.Contains(Editor, StringComparison.OrdinalIgnoreCase))
                .SelectMany(assembly => assembly.GetTypes())
                .ToArray();
        }

        private static List<SearchTreeEntry> CreateSearchTree(List<TreeNode> nodes) {
            var tree = new List<SearchTreeEntry>(nodes.Count);

            for (int i = 0; i < nodes.Count; i++) {
                var node = nodes[i];
                tree.Add(node.isGroup 
                    ? SearchTreeEntryUtils.Header(node.title, node.level) 
                    : SearchTreeEntryUtils.Entry(node.title, node.type, node.level)
                );
            }

            return tree;
        }
    }

}