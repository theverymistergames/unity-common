using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Tree;
using MisterGames.Fsm.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MisterGames.Fsm.Editor.Windows {

    internal class GraphSearchWindow : ScriptableObject, ISearchWindowProvider {

        internal Action<Type, Vector2> onStateCreationRequest = delegate {  };
        internal Action<FsmState, Vector2> onTargetStateCreationRequest = delegate {  };
        internal Action<Type, Vector2> onTransitionCreationRequest = delegate {  };
        internal Func<List<FsmState>> onStatesRequest = () => new List<FsmState>();

        internal Filter filter = Filter.NewState;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            var tree = new List<SearchTreeEntry>();
            
            switch (filter) {
                case Filter.NewState:
                    tree.Add(SearchTreeEntryUtils.Header("Create state"));
                    tree.AddRange(CreateSearchTree<FsmState>(CreateNewStateInfo, 1));
                    break;
                
                case Filter.TargetState:
                    tree.Add(SearchTreeEntryUtils.Header("Add target state"));
                    tree.AddRange(onStatesRequest.Invoke()
                        .Select(s => SearchTreeEntryUtils.Entry(s.name, CreateTargetStateInfo(s), 1))
                    );
                    break;
                
                case Filter.Transition:
                    tree.Add(SearchTreeEntryUtils.Header("Create transition"));
                    tree.AddRange(CreateSearchTree<FsmTransition>(CreateTransitionInfo, 1));
                    break;
            }
            
            return tree;
        }

        private static IEnumerable<SearchTreeEntry> CreateSearchTree<T>(Func<Type, object> getData, int level) {
            var getName = new Func<Type, string>(TypeNameFormatter.GetTypeName);

            var type = typeof(T);
            var types = TypeCache.GetTypesDerivedFrom(type).Where(t => t.IsVisible).ToArray();

            return TypeTree.CreateEntry(type, types, level)
                .RemoveAbstractBranches()
                .SelfChildNonAbstractBranches()
                .SortBranchesInChildrenFirst()
                .PreOrder()
                .RemoveRoot()
                .Select(e => e.ToSearchEntry(getName, getData, level))
                .ToList();
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is Info data) {
                if (data.filter == Filter.NewState) {
                    var type = (Type) data.data;
                    onStateCreationRequest.Invoke(type, context.screenMousePosition);
                    return true;
                }

                if (data.filter == Filter.TargetState) {
                    var state = (FsmState) data.data;
                    onTargetStateCreationRequest.Invoke(state, context.screenMousePosition);
                    return true;
                }

                if (data.filter == Filter.Transition) {
                    var type = (Type) data.data;
                    onTransitionCreationRequest.Invoke(type, context.screenMousePosition);
                    return true;
                }
            }

            return false;
        }

        private object CreateNewStateInfo(Type type) {
            return new Info { filter = Filter.NewState, data = type };
        }

        private object CreateTargetStateInfo(FsmState state) {
            return new Info { filter = Filter.TargetState, data = state };
        }
        
        private object CreateTransitionInfo(Type type) {
            return new Info { filter = Filter.Transition, data = type };
        }
        
        private struct Info {
            public Filter filter;
            public object data;
        }
        
        internal enum Filter {
            NewState,
            TargetState,
            Transition
        }
        
    }

}
