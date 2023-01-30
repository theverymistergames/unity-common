using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Editor.Tree;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Blackboard = MisterGames.Common.Data.Blackboard;

namespace MisterGames.Common.Editor {

    public class BlackboardSearchWindow : ScriptableObject, ISearchWindowProvider {

        public Action<Type> onSelectType = delegate {  };

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header("Select type") };
            tree.AddRange(Blackboard.SupportedTypes.Select(CreateSearchTreeEntry));
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is Type type) {
                onSelectType.Invoke(type);
                return true;
            }
            return false;
        }

        private static SearchTreeEntry CreateSearchTreeEntry(Type type) {
            return SearchTreeEntryUtils.Entry(Blackboard.GetTypeName(type), type, 1);
        }
    }

}
