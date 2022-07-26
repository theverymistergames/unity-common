using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Fsm.Core {

    public abstract class FsmState : ScriptableObjectWithId, IStatePosition {
        
        [HideInInspector] [SerializeField] public List<FsmTransition> transitions = new List<FsmTransition>();
        [SerializeField] public ScriptableObject data;
        
        [HideInInspector] [SerializeField] private Vector2 _position;
        Vector2 IStatePosition.Position { 
            get => _position;
            set => _position = value;
        }

        internal event Action<FsmTransition, FsmState> OnTransit = delegate {  };
        
        protected abstract void OnAttach(StateMachineRunner runner);
        protected abstract void OnDetach();
        protected abstract void OnEnterState();
        protected abstract void OnExitState();

        internal void Attach(StateMachineRunner runner) {
            OnAttach(runner);
            foreach (var transition in transitions) {
                transition.Attach(runner);
            }
        }
        
        internal void Detach() {
            OnDetach();
            foreach (var transition in transitions) {
                transition.Detach();
            }
        }
        
        internal void EnterState() {
            OnEnterState();
            SubscribeTransitions();
        }
        
        internal void ExitState() {
            OnExitState();
            UnsubscribeTransitions();
        }

        private void SubscribeTransitions() {
            foreach (var transition in transitions) {
                transition.EnterSourceState();
                transition.OnTransit += TransitTo;
            }
        }

        private void UnsubscribeTransitions() {
            foreach (var transition in transitions) {
                transition.ExitSourceState();
                transition.OnTransit -= TransitTo;
            }
        }

        private void TransitTo(FsmTransition transition, FsmState state) {
            OnTransit.Invoke(transition, state);
        }

        public override string ToString() {
            return $"{GetType().Name}[{name}](transitions: {transitions.Count})";
        }
        
    }

}