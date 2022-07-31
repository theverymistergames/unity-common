﻿using MisterGames.Character.Input;
 using MisterGames.Character.Pose;
 using MisterGames.Character.Collisions;
 using MisterGames.Common.Collisions;
 using MisterGames.Common.Collisions.Core;
 using MisterGames.Common.Maths;
using MisterGames.Fsm.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public class MotionConditions : MonoBehaviour {

        [SerializeField] private CharacterInput _input;
        [SerializeField] private PoseProcessor _poseProcessor;
        [SerializeField] private CollisionDetector _groundDetector;
        [SerializeField] private StateMachineRunner _motionFsm;
        
        private MotionStateCondition _currentCondition;
        private MotionStateCondition _previousCondition;

        private void OnEnable() {
            _input.Move += HandleMoveInput;
            _input.StartRun += HandleStartRun;
            _input.StopRun += HandleStopRun;
            
            _poseProcessor.OnCrouch += HandleCrouch;
            _poseProcessor.OnStand += HandleStand;

            _groundDetector.OnContact += HandleLanded;
            _groundDetector.OnLostContact += HandleFell;
        }
        
        private void OnDisable() {
            _input.Move -= HandleMoveInput;
            _input.StartRun -= HandleStartRun;
            _input.StopRun -= HandleStopRun;
            
            _poseProcessor.OnCrouch -= HandleCrouch;
            _poseProcessor.OnStand -= HandleStand;

            _groundDetector.OnContact -= HandleLanded;
            _groundDetector.OnLostContact -= HandleFell;
        }

        private void HandleMoveInput(Vector2 input) {
            _currentCondition.isMotionActive = !input.IsNearlyZero();
            InvalidateCondition();
        }

        private void HandleCrouch() {
            _currentCondition.isCrouchActive = true;
            InvalidateCondition();
        }

        private void HandleStand() {
            _currentCondition.isCrouchActive = false;
            InvalidateCondition();
        }

        private void HandleStartRun() {
            _currentCondition.isRunActive = true;
            InvalidateCondition();
        }

        private void HandleStopRun() {
            _currentCondition.isRunActive = false;
            InvalidateCondition();
        }
        
        private void HandleLanded() {
            _currentCondition.isGrounded = true;
            InvalidateCondition();
        }

        private void HandleFell() {
            _currentCondition.isGrounded = false;
            InvalidateCondition();
        }
        
        private void InvalidateCondition() {
            if (_currentCondition.Equals(_previousCondition)) return;
            _previousCondition = _currentCondition;
            
            foreach (var transition in _motionFsm.Instance.CurrentState.transitions) {
                if (transition is MotionStateTransition t && t.CheckCondition(_currentCondition)) {
                    return;
                }
            }
        }

    }

}
