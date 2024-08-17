using System;
using System.Collections.Generic;
using MisterGames.Common.Editor.Tree;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MisterGames.Blackboards.Editor {

    public class BlackboardSearchWindow : ScriptableObject, ISearchWindowProvider {

        public Action<Type> onSelectType = delegate {  };
        public Action<SearchWindowContext> onPendingArrayElementTypeSelection = delegate {  };

        private bool _isPendingArrayElementTypeSelection;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            string title = _isPendingArrayElementTypeSelection ? "Select element type" : "Select type";
            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header(title) };

            if (!_isPendingArrayElementTypeSelection) tree.Add(SearchTreeEntryUtils.Entry("Array", "array", 1));
            tree.AddRange(BlackboardTypeCache.SearchTree);

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

        
    }

}
