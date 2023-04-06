using System;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Core2.Run {

    public class CharacterRunPipeline : MonoBehaviour, ICharacterRunPipeline {

        [SerializeField] private CharacterAccess _characterAccess;

        public event Action OnStartRun = delegate {  };
        public event Action OnStopRun = delegate {  };

        public bool IsRunActive { get; private set; }

        private bool _isEnabled;
        private bool _isCrouchInputActive;

        public void SetEnabled(bool isEnabled) {
            _isEnabled = isEnabled;
            UpdateState();

            if (isEnabled) {
                _characterAccess.Input.OnMotionVectorChanged -= HandleMotionVectorChanged;
                _characterAccess.Input.OnMotionVectorChanged += HandleMotionVectorChanged;

                _characterAccess.Input.RunPressed -= HandleRunPressed;
                _characterAccess.Input.RunPressed += HandleRunPressed;

                _characterAccess.Input.RunReleased -= HandleRunReleased;
                _characterAccess.Input.RunReleased += HandleRunReleased;

                _characterAccess.Input.CrouchPressed -= HandleCrouchPressedInput;
                _characterAccess.Input.CrouchPressed += HandleCrouchPressedInput;

                _characterAccess.Input.CrouchReleased -= HandleCrouchReleasedInput;
                _characterAccess.Input.CrouchReleased += HandleCrouchReleasedInput;
                return;
            }

            _characterAccess.Input.OnMotionVectorChanged -= HandleMotionVectorChanged;

            _characterAccess.Input.RunPressed -= HandleRunPressed;
            _characterAccess.Input.RunReleased -= HandleRunReleased;

            _characterAccess.Input.CrouchPressed -= HandleCrouchPressedInput;
            _characterAccess.Input.CrouchReleased -= HandleCrouchReleasedInput;
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void HandleMotionVectorChanged(Vector2 motion) {
            UpdateState();
        }

        private void HandleRunPressed() {
            UpdateState();
        }

        private void HandleRunReleased() {
            UpdateState();
        }

        private void HandleCrouchPressedInput() {
            UpdateState();
        }

        private void HandleCrouchReleasedInput() {
            UpdateState();
        }

        private void UpdateState() {
            var motionInput = _characterAccess.MotionPipeline.MotionInput;

            bool wasRunActive = IsRunActive;
            IsRunActive =
                _isEnabled &&
                _characterAccess.Input.IsRunPressed &&
                !_characterAccess.Input.IsCrouchPressed &&
                (motionInput.y > Mathf.Epsilon || motionInput.y >= 0f && !motionInput.x.IsNearlyZero());

            if (wasRunActive && !IsRunActive) {
                OnStopRun.Invoke();
                return;
            }

            if (!wasRunActive && IsRunActive) {
                OnStartRun.Invoke();
            }
        }
    }

}
