using System;
using MisterGames.BlueprintLib.Fsm;
using MisterGames.Character.Core2;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.BlueprintLib.Character {

    [Serializable]
    public sealed class CharacterMotionBlueprintFsmTransition : IBlueprintFsmTransition, IBlueprintFsmTransitionDynamicData {

        [Header("Input")]
        [SerializeField] private Optional<bool> _isMotionInputActive;
        [SerializeField] private Optional<bool> _isCrouchInputActive;
        [SerializeField] private Optional<bool> _isCrouchInputToggled;
        [SerializeField] private Optional<bool> _isRunInputToggled;

        [Header("Constraints")]
        [SerializeField] private Optional<bool> _isGrounded;
        [SerializeField] private Optional<bool> _hasCeiling;
        [SerializeField] private Optional<bool> _isMovingForward;
        [SerializeField] private Optional<float> _minHeight;
        [SerializeField] private Optional<float> _maxHeight;

        public Type DataType => typeof(CharacterAccessDynamicData);
        public IDynamicData Data { private get; set; }

        private ICharacterAccess _characterAccess;
        private IBlueprintFsmTransitionCallback _callback;

        private bool _wasCrouchInputToggled;
        private bool _wasRunInputToggled;

        public void Arm(IBlueprintFsmTransitionCallback callback) {
            if (Data is not CharacterAccessDynamicData data) return;

            _characterAccess = data.characterAccess;
            _callback = callback;

            Disarm();

            _characterAccess.Input.OnMotionVectorChanged += HandleMotionInput;

            _characterAccess.Input.CrouchPressed += HandleCrouchPressedInput;
            _characterAccess.Input.CrouchReleased += HandleCrouchReleasedInput;
            _characterAccess.Input.CrouchToggled += HandleCrouchToggledInput;

            _characterAccess.Input.RunToggled += HandleRunToggledInput;

            _characterAccess.GroundDetector.OnContact += HandleLanded;
            _characterAccess.GroundDetector.OnLostContact += HandleFell;

            _characterAccess.HeightPipeline.OnHeightChanged += HandleHeightChange;
        }

        public void Disarm() {
            if (_characterAccess == null) return;

            _characterAccess.Input.OnMotionVectorChanged -= HandleMotionInput;

            _characterAccess.Input.CrouchPressed -= HandleCrouchPressedInput;
            _characterAccess.Input.CrouchReleased -= HandleCrouchReleasedInput;
            _characterAccess.Input.CrouchToggled -= HandleCrouchToggledInput;

            _characterAccess.Input.RunToggled -= HandleRunToggledInput;

            _characterAccess.GroundDetector.OnContact -= HandleLanded;
            _characterAccess.GroundDetector.OnLostContact -= HandleFell;

            _characterAccess.HeightPipeline.OnHeightChanged -= HandleHeightChange;
        }

        private void HandleMotionInput(Vector2 input) {
            TryTransit();
        }

        private void HandleCrouchPressedInput() {
            TryTransit();
        }

        private void HandleCrouchReleasedInput() {
            TryTransit();
        }

        private void HandleCrouchToggledInput() {
            _wasCrouchInputToggled = true;
            TryTransit();
            _wasCrouchInputToggled = false;
        }

        private void HandleRunToggledInput() {
            _wasRunInputToggled = true;
            TryTransit();
            _wasRunInputToggled = false;
        }

        private void HandleLanded() {
            TryTransit();
        }

        private void HandleFell() {
            TryTransit();
        }

        private void HandleHeightChange(float progress, float duration) {
            TryTransit();
        }

        private void TryTransit() {
            if (CanTransit()) _callback.OnTransitionRequested();
        }

        private bool CanTransit() {
            float height = _characterAccess.HeightPipeline.Height;
            var motionInput = _characterAccess.MotionPipeline.MotionInput;

            return _isMotionInputActive.IsEmptyOrEquals(!motionInput.IsNearlyZero()) &&

                   _isCrouchInputActive.IsEmptyOrEquals(_characterAccess.Input.IsCrouchPressed) &&
                   _isCrouchInputToggled.IsEmptyOrEquals(_wasCrouchInputToggled) &&

                   _isRunInputToggled.IsEmptyOrEquals(_wasRunInputToggled) &&

                   _isGrounded.IsEmptyOrEquals(_characterAccess.GroundDetector.CollisionInfo.hasContact) &&
                   _hasCeiling.IsEmptyOrEquals(_characterAccess.CeilingDetector.CollisionInfo.hasContact) &&

                   _isMovingForward.IsEmptyOrEquals(motionInput.y >  0f) &&

                   (!_minHeight.HasValue || _minHeight.Value <= height) &&
                   (!_maxHeight.HasValue || _maxHeight.Value >= height);
        }
    }

}
