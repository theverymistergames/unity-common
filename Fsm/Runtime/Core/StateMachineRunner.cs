using System;
using System.Collections.Generic;
using MisterGames.Tick.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace MisterGames.Fsm.Core {

    /// <summary>
    /// Runs <see cref="StateMachine"/>.
    /// Note that in the runtime runner creates runtime copy of the StateMachine asset on Awake stage
    /// of a MonoBehaviour lifetime, that is referenced in <see cref="Instance"/> property.
    /// So, you can not operate with runtime instance on Awake, OnEnable and OnDisable stages,
    /// do stuff on Start stage at least.  
    /// </summary>
    public sealed class StateMachineRunner : MonoBehaviour {

        [SerializeField] private TimeDomain timeDomain;
        [SerializeField] private StateMachine _stateMachine;

        public event Action<FsmState> OnEnterState = delegate {  }; 
        public event Action<FsmState> OnExitState = delegate {  };
        
        public StateMachine Source => _stateMachine;
        public StateMachine Instance { get; private set; }
        public ITimeSource TimeSource => timeDomain.Source;

        private void Awake() {
            Instance = CloneInstance(_stateMachine);
            Instance.ArmInitialState();
        }

        private void OnDestroy() {
            Destroy(Instance);
        }

        private void OnEnable() {
            Instance.Attach(this);
            
            Instance.OnEnterState += HandleEnterState;
            Instance.OnExitState += HandleExitState;
            
            Instance.EnterCurrentState();
        }
        
        private void OnDisable() {
            Instance.ExitCurrentState();
            
            Instance.OnEnterState -= HandleEnterState;
            Instance.OnExitState -= HandleExitState;
            
            Instance.Detach();
        }

        private void HandleEnterState(FsmState state) {
            OnEnterState.Invoke(state);
        }

        private void HandleExitState(FsmState state) {
            OnExitState.Invoke(state);
        }
        
        private static StateMachine CloneInstance(StateMachine source) {
            Assert.IsNotNull(source, "State machine not set");
            
            var states = source.states;
            Assert.IsTrue(states.Count > 0, $"No states found in {source}");

            var initialState = source.initialState;
            int initialStateIndex = states.IndexOf(initialState);
            Assert.IsTrue(initialState != null && initialStateIndex >= 0, $"Initial state not set in {source}");
            
            var stateMachine = Instantiate(source);
            stateMachine.name = $"{source.name} (Runtime copy)";
            
            stateMachine.states = new List<FsmState>();
            for (int i = 0; i < states.Count; i++) {
                var state = states[i];
                var clonedState = Instantiate(state);
                clonedState.name = state.name;
                stateMachine.states.Add(clonedState);
            }

            for (int i = 0; i < stateMachine.states.Count; i++) {
                var state = states[i];
                var clonedState = stateMachine.states[i];
                
                clonedState.transitions = new List<FsmTransition>();
                for (int j = 0; j < state.transitions.Count; j++) {
                    var transition = state.transitions[j];
                    var clonedTransition = Instantiate(transition);
                    clonedTransition.name = transition.name;

                    int targetStateIndex = states.IndexOf(transition.targetState);
                    clonedTransition.targetState = stateMachine.states[targetStateIndex];

                    clonedState.transitions.Add(clonedTransition);
                }
            }

            stateMachine.initialState = stateMachine.states[initialStateIndex];
            return stateMachine;
        }

    }

}
