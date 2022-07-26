﻿using System;
 using MisterGames.Character.Configs;
 using MisterGames.Fsm.Core;
 using UnityEngine;

 namespace MisterGames.Character.Motion {

    public class RunProcessor : MonoBehaviour {

        [SerializeField] private StateMachineRunner _motionFsm;

        public event Action OnStartRun = delegate {  };
        public event Action OnStopRun = delegate {  };
        
        public bool IsRunning { get; private set; }
        
        private void OnEnable() {
            _motionFsm.OnEnterState += HandleMotionStateChanged;
        }

        private void OnDisable() {
            _motionFsm.OnEnterState -= HandleMotionStateChanged;
        }

        private void Start() {
            HandleMotionStateChanged(_motionFsm.Instance.CurrentState);
        }

        private void HandleMotionStateChanged(FsmState state) {
            if (state.data is MotionStateData data) { 
                SetState(data.isRunState);    
            }
        }

        private void SetState(bool isRunning) {
            var wasRunning = IsRunning;
            IsRunning = isRunning;
            
            if (wasRunning && !isRunning) {
                OnStopRun.Invoke();
                return;
            }
            if (!wasRunning && isRunning) {
                OnStartRun.Invoke();
            }
        }

    }

}