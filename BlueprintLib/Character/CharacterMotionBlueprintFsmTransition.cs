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

        public void Arm(IBlueprintFsmTransitionCallback callback) {
            if (Data is not CharacterMotionBlueprintFsmTransitionData data) return;

            _characterAccess = data.characterAccess;
            _callback = callback;

            Disarm();

            _characterAccess.Input.OnMotionVectorChanged += HandleMotionInput;

            _characterAccess.Input.CrouchPressed += HandleCrouchPressedInput;
            _characterAccess.Input.CrouchReleased += HandleCrouchReleasedInput;

            _characterAccess.RunPipeline.OnStartRun += HandleCharacterStartRun;
            _characterAccess.RunPipeline.OnStopRun += HandleCharacterStopRun;

            _characterAccess.GroundDetector.OnContact += OnLanded;
            _characterAccess.GroundDetector.OnLostContact += OnFell;
        }

        public void Disarm() {
            if (_characterAccess == null) return;

            _characterAccess.Input.OnMotionVectorChanged -= HandleMotionInput;

            _characterAccess.Input.CrouchPressed -= HandleCrouchPressedInput;
            _characterAccess.Input.CrouchReleased -= HandleCrouchReleasedInput;

            _characterAccess.RunPipeline.OnStartRun -= HandleCharacterStartRun;
            _characterAccess.RunPipeline.OnStopRun -= HandleCharacterStopRun;

            _characterAccess.GroundDetector.OnContact -= OnLanded;
            _characterAccess.GroundDetector.OnLostContact -= OnFell;
        }

        private void HandleMotionInput(Vector2 input) {
            TryTransit();
        }

        private void HandleCharacterStartRun() {
            TryTransit();
        }

        private void HandleCharacterStopRun() {
            TryTransit();
        }

        private void HandleCrouchPressedInput() {
            TryTransit();
        }

        private void HandleCrouchReleasedInput() {
            TryTransit();
        }

        private void OnLanded() {
            TryTransit();
        }

        private void OnFell() {
            TryTransit();
        }

        private void TryTransit() {
            if (CanTransit()) _callback.OnTransitionRequested();
        }

        private bool CanTransit() {
            return _isMotionActive.IsEmptyOrEquals(!_characterAccess.MotionPipeline.MotionInput.IsNearlyZero()) &&
                   _isRunActive.IsEmptyOrEquals(_characterAccess.RunPipeline.IsRunActive) &&
                   _isCrouchActive.IsEmptyOrEquals(_characterAccess.Input.IsCrouchPressed) &&
                   _isGrounded.IsEmptyOrEquals(_characterAccess.GroundDetector.CollisionInfo.hasContact);
        }
    }

}
