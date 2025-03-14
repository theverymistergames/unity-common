using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Blackboards.Tables;
using MisterGames.Common.Trees;
using MisterGames.Common.Types;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Editor {
    
    [InitializeOnLoad]
    public static class BlackboardTypeCache {
 
        public static List<SearchTreeEntry> SearchTree { get; }

        static BlackboardTypeCache() {
            SearchTree = CreateTypeTree();
        }

        private static List<SearchTreeEntry> CreateTypeTree() {
            var tree = new List<SearchTreeEntry>();

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
                .Where(t => t.IsClass && !typeof(UnityEngine.Object).IsAssignableFrom(t) && BlackboardTableUtils.IsSupportedElementType(t))
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
        
        private static IEnumerable<SearchTreeEntry> GetTypeTree(string rootName, IReadOnlyList<Type> types, int level) {
            if (types.Count == 0) return Array.Empty<SearchTreeEntry>();

            if (types.Count == 1) {
                var type = types[0];
                return new List<SearchTreeEntry> {
                    SearchTreeEntryUtils.Entry(TypeNameFormatter.GetFullTypeNamePathInBraces(type), type, level)
                };
            }

            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header(rootName, level) };

            var pathTree = PathTree
                .CreateTree(types, t => t.FullName, '.')
                .PreOrder()
                .Where(e => !string.IsNullOrEmpty(e.data.name))
                .Select(e => ToSearchEntry(e, level));

            tree.AddRange(pathTree);

            return tree;
        }

        private static SearchTreeEntry ToSearchEntry(TreeEntry<PathTree.Node<Type>> treeEntry, int levelOffset) {
            if (treeEntry.children.Count == 0) {
                var type = treeEntry.data.data;
                return SearchTreeEntryUtils.Entry(
                    TypeNameFormatter.GetFullTypeNamePathInBraces(type),
                    type,
                    treeEntry.level + levelOffset
                );
            }

            return SearchTreeEntryUtils.Header(treeEntry.data.name, treeEntry.level + levelOffset);
        }
        
        private static Type[] CollectAssemblyTypes() {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.FullName.Contains("editor", StringComparison.OrdinalIgnoreCase))
                .SelectMany(assembly => assembly.GetTypes())
                .ToArray();   
        }
    }
    
}