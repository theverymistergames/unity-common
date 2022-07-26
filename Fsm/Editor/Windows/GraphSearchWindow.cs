using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Editor.Reflect;
using MisterGames.Common.Trees;
using MisterGames.Fsm.Core;
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
            var builder = SearchTree.Create();
            
            switch (filter) {
                case Filter.NewState:
                    builder
                        .Add(SearchTree.Header("Create state"))
                        .Add(TypeTree.CreateSearchTree<FsmState>(NewStateInfo));
                    break;
                
                case Filter.TargetState:
                    builder
                        .Add(SearchTree.Header("Add target state"))
                        .Add(onStatesRequest
                            .Invoke()
                            .Select(s => SearchTree.Entry(s.name, TargetStateInfo(s), 1))
                        );
                    break;
                
                case Filter.Transition:
                    builder
                        .Add(SearchTree.Header("Create transition"))
                        .Add(TypeTree.CreateSearchTree<FsmTransition>(TransitionInfo));
                    break;
            }
            
            return builder.Build();
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

        private object NewStateInfo(Type type) {
            return new Info { filter = Filter.NewState, data = type };
        }

        private object TargetStateInfo(FsmState state) {
            return new Info { filter = Filter.TargetState, data = state };
        }
        
        private object TransitionInfo(Type type) {
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