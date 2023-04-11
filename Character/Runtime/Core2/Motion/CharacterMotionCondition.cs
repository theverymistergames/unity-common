using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Core2.Motion {
    [Serializable]
    public sealed class CharacterMotionCondition : ICondition, IDynamicDataHost {

        [Header("Inputs")]
        [SerializeField] private Optional<bool> _isMotionInputActive;
        [SerializeField] private Optional<bool> _isCrouchInputActive;
        [SerializeField] private Optional<bool> _isCrouchInputToggled;
        [SerializeField] private Optional<bool> _isRunInputToggled;

        [Header("Constraints")]
        [SerializeField] private Optional<bool> _isMovingForward;
        [SerializeField] private Optional<bool> _isGrounded;
        [SerializeField] private Optional<bool> _hasCeiling;
        [SerializeField] private Optional<float> _minHeight;
        [SerializeField] private Optional<float> _maxHeight;

        public Type DataType => typeof(CharacterAccess);
        public bool IsMatched => CheckCondition();

        private IConditionCallback _callback;
        private ICharacterAccess _characterAccess;

        private bool _wasCrouchInputToggled;
        private bool _wasRunInputToggled;

        public void OnSetData(IDynamicDataProvider provider) {
            _characterAccess = provider.GetData<CharacterAccess>();
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            if (_isMotionInputActive.HasValue || _isMovingForward.HasValue) {
                _characterAccess.Input.OnMotionVectorChanged += OnMotionVectorChanged;
            }

            if (_isCrouchInputActive.HasValue) {
                _characterAccess.Input.CrouchPressed += OnCrouchPressed;
                _characterAccess.Input.CrouchReleased += OnCrouchReleased;
            }

            if (_isCrouchInputToggled.HasValue) {
                _characterAccess.Input.CrouchToggled += OnCrouchToggled;
            }

            if (_isRunInputToggled.HasValue) {
                _characterAccess.Input.RunToggled += OnRunToggled;
            }

            if (_minHeight.HasValue || _maxHeight.HasValue) {
                _characterAccess.HeightPipeline.OnHeightChanged += OnHeightChanged;
            }

            if (_isGrounded.HasValue) {
                _characterAccess.GroundDetector.OnContact += OnGrounded;
                _characterAccess.GroundDetector.OnLostContact += OnFell;
                _characterAccess.GroundDetector.OnTransformChanged += OnGroundTransformChanged;
            }

            if (_hasCeiling.HasValue) {
                _characterAccess.CeilingDetector.OnContact += OnCeilingAppeared;
                _characterAccess.CeilingDetector.OnLostContact += OnCeilingDisappeared;
            }
        }

        public void Disarm() {
            if (_isMotionInputActive.HasValue || _isMovingForward.HasValue) {
                _characterAccess.Input.OnMotionVectorChanged -= OnMotionVectorChanged;
            }

            if (_isCrouchInputActive.HasValue) {
                _characterAccess.Input.CrouchPressed -= OnCrouchPressed;
                _characterAccess.Input.CrouchReleased -= OnCrouchReleased;
            }

            if (_isCrouchInputToggled.HasValue) {
                _characterAccess.Input.CrouchToggled -= OnCrouchToggled;
            }

            if (_isRunInputToggled.HasValue) {
                _characterAccess.Input.RunToggled -= OnRunToggled;
            }

            if (_minHeight.HasValue || _maxHeight.HasValue) {
                _characterAccess.HeightPipeline.OnHeightChanged -= OnHeightChanged;
            }

            if (_isGrounded.HasValue) {
                _characterAccess.GroundDetector.OnContact -= OnGrounded;
                _characterAccess.GroundDetector.OnLostContact -= OnFell;
                _characterAccess.GroundDetector.OnTransformChanged -= OnGroundTransformChanged;
            }

            if (_hasCeiling.HasValue) {
                _characterAccess.CeilingDetector.OnContact -= OnCeilingAppeared;
                _characterAccess.CeilingDetector.OnLostContact -= OnCeilingDisappeared;
            }

            _callback = null;
        }

        private void OnMotionVectorChanged(Vector2 motionVector) {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnCrouchPressed() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnCrouchReleased() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnCrouchToggled() {
            _wasCrouchInputToggled = true;
            if (IsMatched) _callback?.OnConditionMatch();
            _wasCrouchInputToggled = false;
        }

        private void OnRunToggled() {
            _wasRunInputToggled = true;
            if (IsMatched) _callback?.OnConditionMatch();
            _wasRunInputToggled = false;
        }

        private void OnHeightChanged(float progress, float duration) {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnGrounded() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnFell() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnGroundTransformChanged() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnCeilingAppeared() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnCeilingDisappeared() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            var motionInput = _characterAccess.MotionPipeline.MotionInput;

            return _isMotionInputActive.IsEmptyOrEquals(!motionInput.IsNearlyZero()) &&
                   _isCrouchInputActive.IsEmptyOrEquals(_characterAccess.Input.IsCrouchPressed) &&
                   _isCrouchInputToggled.IsEmptyOrEquals(_wasCrouchInputToggled) &&
                   _isRunInputToggled.IsEmptyOrEquals(_wasRunInputToggled) &&

                   _isMovingForward.IsEmptyOrEquals(motionInput.y > 0f) &&

                   _isGrounded.IsEmptyOrEquals(_characterAccess.GroundDetector.CollisionInfo.hasContact) &&
                   _hasCeiling.IsEmptyOrEquals(_characterAccess.CeilingDetector.CollisionInfo.hasContact) &&

                   (!_minHeight.HasValue || _minHeight.Value <= _characterAccess.HeightPipeline.Height) &&
                   (!_maxHeight.HasValue || _maxHeight.Value >= _characterAccess.HeightPipeline.Height);
        }
    }

}
