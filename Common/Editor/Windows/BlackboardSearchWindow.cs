using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Reflect;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Blackboard = MisterGames.Common.Data.Blackboard;

namespace MisterGames.Fsm.Editor.Windows {

    public class BlackboardSearchWindow : ScriptableObject, ISearchWindowProvider {

        public Action<Type> onSelectType = delegate {  };

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            return SearchTree.Create()
                .Add(SearchTree.Header("Select type"))
                .Add(Blackboard.SupportedTypes.Select(CreateEntry))
                .Build();
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is Type type) {
                onSelectType.Invoke(type);
                return true;
            }
            return false;
        }

        private static SearchTreeEntry CreateEntry(Type type) {
            return SearchTree.Entry(Blackboard.GetTypeName(type), type, 1);
        }
        
    }

}