using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Editor.Tree;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Blackboard = MisterGames.Common.Data.Blackboard;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Editor.Blackboards {

    public class BlackboardSearchWindow : ScriptableObject, ISearchWindowProvider {

        public Action<Type> onSelectType = delegate {  };

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header("Select type") };

            var types = TypeCache
                .GetTypesDerivedFrom<ScriptableObject>()
                .Where(Blackboard.IsSupportedType)
                .ToList();
            types.Add(typeof(ScriptableObject));
            tree.AddRange(GetTypeTree(typeof(ScriptableObject).FullName, types, 1));

            types = TypeCache
                .GetTypesDerivedFrom<Component>()
                .Where(Blackboard.IsSupportedType)
                .ToList();
            types.Add(typeof(Component));
            tree.AddRange(GetTypeTree(typeof(Component).FullName, types, 1));

            types = TypeCache
                .GetTypesDerivedFrom<object>()
                .Where(t => !typeof(Object).IsAssignableFrom(t) && Blackboard.IsSupportedType(t))
                .ToList();
            tree.AddRange(GetTypeTree(typeof(object).FullName, types, 1));

            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsEnum && Blackboard.IsSupportedType(t))
                .ToList();
            tree.AddRange(GetTypeTree(typeof(Enum).FullName, types, 1));

            tree.AddRange(Blackboard.RootSearchFolderTypes.Select(t => CreateSearchTreeEntryForType(t, 1)));

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is Type type) {
                onSelectType.Invoke(type);
                return true;
            }
            return false;
        }

        private static IEnumerable<SearchTreeEntry> GetTypeTree(string rootName, IReadOnlyList<Type> types, int level) {
            if (types.Count == 0) return new List<SearchTreeEntry>();

            if (types.Count == 1) {
                var type = types[0];
                return new List<SearchTreeEntry> { SearchTreeEntryUtils.Entry(type.Name, type, level) };
            }

            var pathTree = PathTree
                .CreateTree(types, t => t.FullName, '.')
                .PreOrder()
                .Where(e => e.level > 0)
                .Select(e => ToSearchEntry(e, level));

            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header(rootName, level) };
            tree.AddRange(pathTree);
            return tree;
        }

        private static SearchTreeEntry CreateSearchTreeEntryForType(Type type, int level) {
            return SearchTreeEntryUtils.Entry(TypeNameFormatter.GetTypeName(type), type, level);
        }

        private static SearchTreeEntry ToSearchEntry<T>(TreeEntry<PathTree.Node<T>> treeEntry, int levelOffset) {
            return treeEntry.children.Count == 0
                ? SearchTreeEntryUtils.Entry(treeEntry.data.name, treeEntry.data.data, treeEntry.level + levelOffset)
                : SearchTreeEntryUtils.Header(treeEntry.data.name, treeEntry.level + levelOffset);
        }
    }

}
