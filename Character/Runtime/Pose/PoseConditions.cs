using MisterGames.Character.Configs;
using MisterGames.Character.Core2.Collisions;
using MisterGames.Character.Input;
using MisterGames.Fsm.Core;
using UnityEngine;

namespace MisterGames.Character.Pose {

    public class PoseConditions : MonoBehaviour {

        [SerializeField] private CharacterInput _input;
        [SerializeField] private StateMachineRunner _poseFsm;
        [SerializeField] private CharacterGroundDetector _groundDetector;
        [SerializeField] private CharacterCeilingDetector _ceilingDetector;

        private PoseStateCondition _currentCondition;
        private PoseStateCondition _previousCondition;
        
        private void OnEnable() {
            _input.StartCrouch += HandleStartCrouchInput;
            _input.StopCrouch += HandleStopCrouchInput;
            _input.ToggleCrouch += HandleToggleCrouchInput;

            _groundDetector.OnContact += HandleLanded;
            _groundDetector.OnLostContact += HandleFell;

            _ceilingDetector.OnContact += HandleCeilingAppeared;
            _ceilingDetector.OnLostContact += HandleCeilingGone;
            
            _poseFsm.OnEnterState += HandleStateChanged;
        }

        private void OnDisable() {
            _input.StartCrouch -= HandleStartCrouchInput;
            _input.StopCrouch -= HandleStopCrouchInput;
            _input.ToggleCrouch -= HandleToggleCrouchInput;

            _groundDetector.OnLostContact -= HandleFell;
            _groundDetector.OnContact -= HandleLanded;

            _ceilingDetector.OnContact -= HandleCeilingAppeared;
            _ceilingDetector.OnLostContact -= HandleCeilingGone;

            _poseFsm.OnEnterState -= HandleStateChanged;
        }

        private void Start() {
            HandleStateChanged(_poseFsm.Instance.CurrentState);
        }

        private void HandleStateChanged(FsmState state) {
            if (state.data is PoseStateData data) {
                _currentCondition.isCrouching = data.isCrouchState;
            }
        }

        private void HandleCeilingAppeared() {
            _currentCondition.hasCeiling = true;
            InvalidateCondition();
        }
        
        private void HandleCeilingGone() {
            _currentCondition.hasCeiling = false;
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
        
        private void HandleStartCrouchInput() {
            _currentCondition.isCrouchInputActive = true;
            InvalidateCondition();
        }

        private void HandleStopCrouchInput() {
            _currentCondition.isCrouchInputActive = false;
            InvalidateCondition();
        }
        
        private void HandleToggleCrouchInput() {
            if (_currentCondition.isCrouching && _currentCondition.isGrounded && _currentCondition.hasCeiling) {
                return;
            } 
            
            _currentCondition.isCrouchInputActive = !_currentCondition.isCrouchInputActive;
            InvalidateCondition();
        }
        
        private void InvalidateCondition() {
            if (_currentCondition.Equals(_previousCondition)) return;
            _previousCondition = _currentCondition;
            
            foreach (var transition in _poseFsm.Instance.CurrentState.transitions) {
                if (transition is PoseStateTransition t && t.CheckCondition(_currentCondition)) {
                    return;
                }
            }
        }

    }

}
