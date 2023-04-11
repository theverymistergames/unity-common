using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;

namespace MisterGames.Character.Core2.Input {

    [Serializable]
    public sealed class CharacterConditionCrouchInput : ICondition {

        public Optional<bool> _isCrouchInputActive;
        public Optional<bool> _isCrouchInputToggled;

        public bool IsMatched => CheckCondition(
            CharacterAccessProvider.CharacterAccess.Input.IsCrouchPressed,
            _wasCrouchInputToggled
        );

        private IConditionCallback _callback;
        private bool _wasCrouchInputToggled;

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            if (_isCrouchInputActive.HasValue) {
                CharacterAccessProvider.CharacterAccess.Input.CrouchPressed += OnCrouchPressed;
                CharacterAccessProvider.CharacterAccess.Input.CrouchReleased += OnCrouchReleased;
            }

            if (_isCrouchInputToggled.HasValue) {
                CharacterAccessProvider.CharacterAccess.Input.CrouchToggled += OnCrouchToggled;
            }
        }

        public void Disarm() {
            if (_isCrouchInputActive.HasValue) {
                CharacterAccessProvider.CharacterAccess.Input.CrouchPressed -= OnCrouchPressed;
                CharacterAccessProvider.CharacterAccess.Input.CrouchReleased -= OnCrouchReleased;
            }

            if (_isCrouchInputToggled.HasValue) {
                CharacterAccessProvider.CharacterAccess.Input.CrouchToggled -= OnCrouchToggled;
            }

            _callback = null;
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

        private bool CheckCondition(bool isCrouchInputActive, bool isCrouchInputToggled) {
            return _isCrouchInputActive.IsEmptyOrEquals(isCrouchInputActive) &&
                   _isCrouchInputToggled.IsEmptyOrEquals(isCrouchInputToggled);
        }
    }

}
