﻿﻿﻿using System;
using MisterGames.Common.Data;
using UnityEngine;
   
namespace MisterGames.Fsm.Core {

    public abstract class FsmTransition : ScriptableObjectWithId, IStatePosition {
        
        [HideInInspector] public FsmState targetState;
        public ScriptableObject data;

        [HideInInspector] [SerializeField] private Vector2 _position;
        Vector2 IStatePosition.Position { 
            get => _position;
            set => _position = value;
        }

        internal event Action<FsmTransition, FsmState> OnTransit = delegate {  };
        protected bool IsValid { get; private set; }

        protected abstract void OnAttach(StateMachineRunner runner);
        protected abstract void OnDetach();
        protected abstract void OnEnterSourceState();
        protected abstract void OnExitSourceState();
        
        internal void Attach(StateMachineRunner runner) {
            OnAttach(runner);
        }
        
        internal void Detach() {
            OnDetach();
        }
        
        internal void EnterSourceState() {
            IsValid = true;
            OnEnterSourceState();
        }

        internal void ExitSourceState() {
            IsValid = false;
            OnExitSourceState();
        }

        protected void Transit() {
            if (IsValid) OnTransit.Invoke(this, targetState);
        }
        
        public override string ToString() {
            return $"FsmTransition[{GetType().Name}](targetState: {targetState})";
        }
        
    }

}