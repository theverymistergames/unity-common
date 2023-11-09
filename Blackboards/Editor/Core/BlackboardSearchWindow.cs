using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MisterGames.Blackboards.Tables;
using MisterGames.Common.Editor.Tree;
using MisterGames.Common.Types;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Blackboard = MisterGames.Blackboards.Core.Blackboard;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Editor {

    public class BlackboardSearchWindow : ScriptableObject, ISearchWindowProvider {

        public Action<Type> onSelectType = delegate {  };
        public Action<SearchWindowContext> onPendingArrayElementTypeSelection = delegate {  };

        private List<SearchTreeEntry> _typeTree;
        private bool _isPendingArrayElementTypeSelection;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            _typeTree ??= CreateTypeTree();

            string title = _isPendingArrayElementTypeSelection ? "Select element type" : "Select type";
            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header(title) };

            if (!_isPendingArrayElementTypeSelection) tree.Add(SearchTreeEntryUtils.Entry("Array", "array", 1));
            tree.AddRange(_typeTree);

            return tree;
        }

        private static List<SearchTreeEntry> CreateTypeTree() {
            var tree = new List<SearchTreeEntry>();

            var assemblyTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.FullName.Contains("editor", StringComparison.OrdinalIgnoreCase))
                .SelectMany(assembly => assembly.GetTypes())
                .ToArray();

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

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is string s) {
                if (s == "array") {
                    _isPendingArrayElementTypeSelection = true;
                    onPendingArrayElementTypeSelection.Invoke(context);
                    return true;
                }

                return false;
            }

            if (searchTreeEntry.userData is Type type) {
                if (_isPendingArrayElementTypeSelection) {
                    _isPendingArrayElementTypeSelection = false;
                    type = type.MakeArrayType();
                }

                onSelectType.Invoke(type);
                return true;
            }

            return false;
        }

        private static IEnumerable<SearchTreeEntry> GetTypeTree(string rootName, IReadOnlyList<Type> types, int level) {
            if (types.Count == 0) return new List<SearchTreeEntry>();

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
    }

}
