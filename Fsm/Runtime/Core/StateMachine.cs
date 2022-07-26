using System;
using System.Collections.Generic;
using MisterGames.Fsm.Basics;
using UnityEngine;

namespace MisterGames.Fsm.Core {

    [CreateAssetMenu(fileName = nameof(StateMachine), menuName = "MisterGames/" + nameof(StateMachine))]
    public sealed class StateMachine : ScriptableObject {
        
        [HideInInspector]
        public FsmState initialState;
        
        [HideInInspector]
        public List<FsmState> states;

        internal event Action<FsmState> OnEnterState = delegate {  }; 
        internal event Action<FsmState> OnExitState = delegate {  };

        public FsmState CurrentState { get; private set; }
        public FsmTransition LastTransition { get; private set; }

        internal void Attach(StateMachineRunner runner) {
            for (var i = 0; i < states.Count; i++) {
                states[i].Attach(runner);
            }
        }

        internal void Detach() {
            for (var i = 0; i < states.Count; i++) {
                states[i].Detach();
            }
        }

        internal void ArmInitialState() {
            CurrentState = initialState;
            LastTransition = CreateInstance<EmptyTransition>();
            LastTransition.targetState = initialState;
        }
        
        internal void EnterCurrentState() {
            CurrentState.OnTransit += TransitTo;
            CurrentState.EnterState();
            OnEnterState.Invoke(CurrentState);
        }

        internal void ExitCurrentState() {
            CurrentState.OnTransit -= TransitTo;
            CurrentState.ExitState();
            OnExitState.Invoke(CurrentState);
        }

        private void EnterNewState(FsmState state) {
            CurrentState = state;
            EnterCurrentState();
        }

        private void TransitTo(FsmTransition transition, FsmState state) {
            ExitCurrentState();
            LastTransition = transition;
            EnterNewState(state);
        }

        public override string ToString() {
            return $"StateMachine[{name}](states: {states?.Count ?? 0})";
        }
        
    }

}
