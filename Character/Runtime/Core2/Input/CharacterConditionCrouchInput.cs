using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;

namespace MisterGames.Character.Core2.Input {

    [Serializable]
    public sealed class CharacterConditionCrouchInput : ICondition, IDynamicDataHost {

        public Optional<bool> _isCrouchInputActive;
        public Optional<bool> _isCrouchInputToggled;

        public bool IsMatched => CheckCondition(
            _characterAccess.Input.IsCrouchPressed,
            _wasCrouchInputToggled
        );

        private ICharacterAccess _characterAccess;
        private IConditionCallback _callback;
        private bool _wasCrouchInputToggled;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _characterAccess = provider.GetData<CharacterAccess>();
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;
            if (IsMatched) _callback?.OnConditionMatch();

            if (_isCrouchInputActive.HasValue) {
                _characterAccess.Input.CrouchPressed += OnCrouchPressed;
                _characterAccess.Input.CrouchReleased += OnCrouchReleased;
            }

            if (_isCrouchInputToggled.HasValue) {
                _characterAccess.Input.CrouchToggled += OnCrouchToggled;
            }
        }

        public void Disarm() {
            if (_isCrouchInputActive.HasValue) {
                _characterAccess.Input.CrouchPressed -= OnCrouchPressed;
                _characterAccess.Input.CrouchReleased -= OnCrouchReleased;
            }

            if (_isCrouchInputToggled.HasValue) {
                _characterAccess.Input.CrouchToggled -= OnCrouchToggled;
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
