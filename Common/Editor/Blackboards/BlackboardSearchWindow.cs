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
            tree.AddRange(GetSupportedDerivedTypesTree(1));
            tree.AddRange(GetSupportedConcreteTypesTree(1));
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is Type type) {
                onSelectType.Invoke(type);
                return true;
            }
            return false;
        }

        private static IEnumerable<SearchTreeEntry> GetSupportedConcreteTypesTree(int level) {
            return Blackboard.SupportedConcreteTypes.Select(t => CreateSearchTreeEntryForType(t, level));
        }

        private static IEnumerable<SearchTreeEntry> GetSupportedDerivedTypesTree(int level) {
            return Blackboard.SupportedDerivedTypes.SelectMany(t => GetSupportedBaseTypeTree(t, level));
        }

        private static IEnumerable<SearchTreeEntry> GetSupportedBaseTypeTree(Type type, int level) {
            var types = new List<Type>();
            if (Blackboard.IsSupportedDerivedType(type)) types.Add(type);
            types.AddRange(TypeCache.GetTypesDerivedFrom(type).Where(Blackboard.IsSupportedDerivedType));

            var pathTree = PathTree
                .CreateTree(types, t => t.FullName, '.')
                .PreOrder()
                .Where(e => e.level > 0)
                .Select(e => ToSearchEntry(e, level));

            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header(type.Name, level) };
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
