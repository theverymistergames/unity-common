using System;
using MisterGames.BlueprintLib.Fsm;
using MisterGames.Character.Core2;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.BlueprintLib.Character {

    [Serializable]
    public sealed class CharacterMotionBlueprintFsmTransitionData : IDynamicData {
        public CharacterAccess characterAccess;
    }

    [Serializable]
    public sealed class CharacterMotionBlueprintFsmTransition : IBlueprintFsmTransition, IBlueprintFsmTransitionDynamicData {

        [SerializeField] private Optional<bool> _isMotionActive;
        [SerializeField] private Optional<bool> _isRunActive;
        [SerializeField] private Optional<bool> _isCrouchActive;
        [SerializeField] private Optional<bool> _isGrounded;

        public Type DataType => typeof(CharacterMotionBlueprintFsmTransitionData);
        public IDynamicData Data { private get; set; }

        private ICharacterAccess _characterAccess;
        private IBlueprintFsmTransitionCallback _callback;

        private bool _motionState;
        private bool _runState;
        private bool _crouchState;
        private bool _groundedState;

        public void Arm(IBlueprintFsmTransitionCallback callback) {
            if (Data is not CharacterMotionBlueprintFsmTransitionData data) return;

            _characterAccess = data.characterAccess;
            _callback = callback;

            Disarm();

            _characterAccess.Input.Move += HandleMotionInput;
            _characterAccess.Input.StartCrouch += HandleStartCrouchInput;
            _characterAccess.Input.StopCrouch += HandleStopCrouchInput;
            _characterAccess.Input.StartRun += HandleStartRunInput;
            _characterAccess.Input.StopRun += HandleStopRunInput;

            _characterAccess.GroundDetector.OnContact += OnLanded;
            _characterAccess.GroundDetector.OnLostContact += OnFell;
        }

        public void Disarm() {
            if (_characterAccess == null) return;

            _characterAccess.Input.Move -= HandleMotionInput;
            _characterAccess.Input.StartCrouch -= HandleStartCrouchInput;
            _characterAccess.Input.StopCrouch -= HandleStopCrouchInput;
            _characterAccess.Input.StartRun -= HandleStartRunInput;
            _characterAccess.Input.StopRun -= HandleStopRunInput;
        }

        private void HandleMotionInput(Vector2 input) {
            _motionState = !input.IsNearlyZero();
            TryTransit();
        }

        private void HandleStartRunInput() {
            _runState = true;
            TryTransit();
        }

        private void HandleStopRunInput() {
            _runState = false;
            TryTransit();
        }

        private void HandleStartCrouchInput() {
            _crouchState = true;
            TryTransit();
        }

        private void HandleStopCrouchInput() {
            _crouchState = false;
            TryTransit();
        }

        private void OnLanded() {
            _groundedState = true;
            TryTransit();
        }

        private void OnFell() {
            _groundedState = false;
            TryTransit();
        }

        private void TryTransit() {
            if (CanTransit()) _callback.OnTransitionRequested();
        }

        private bool CanTransit() {
            return _isMotionActive.IsEmptyOrEquals(_motionState) &&
                   _isRunActive.IsEmptyOrEquals(_runState) &&
                   _isCrouchActive.IsEmptyOrEquals(_crouchState) &&
                   _isGrounded.IsEmptyOrEquals(_groundedState);
        }
    }

}
