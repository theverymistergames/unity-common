using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;

namespace MisterGames.Character.Core2.Input {

    [Serializable]
    public sealed class CharacterConditionCrouchInput : ICondition, IDynamicDataHost {

        public Optional<bool> _isCrouchInputActive;
        public Optional<bool> _isCrouchInputPressed;
        public Optional<bool> _isCrouchInputReleased;
        public Optional<bool> _isCrouchInputToggled;

        public bool IsMatched => CheckCondition();

        private ICharacterAccess _characterAccess;
        private IConditionCallback _callback;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _characterAccess = provider.GetData<CharacterAccess>();
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            if (_characterAccess != null) {
                if (_isCrouchInputActive.HasValue || _isCrouchInputPressed.HasValue) {
                    _characterAccess.Input.CrouchPressed += OnCrouchPressed;
                }

                if (_isCrouchInputActive.HasValue || _isCrouchInputReleased.HasValue) {
                    _characterAccess.Input.CrouchReleased += OnCrouchReleased;
                }

                if (_isCrouchInputToggled.HasValue) {
                    _characterAccess.Input.CrouchToggled += OnCrouchToggled;
                }
            }

            if (IsMatched) _callback?.OnConditionMatch();
        }

        public void Disarm() {
            if (_characterAccess != null) {
                if (_isCrouchInputActive.HasValue || _isCrouchInputPressed.HasValue) {
                    _characterAccess.Input.CrouchPressed -= OnCrouchPressed;
                }

                if (_isCrouchInputActive.HasValue || _isCrouchInputReleased.HasValue) {
                    _characterAccess.Input.CrouchReleased -= OnCrouchReleased;
                }

                if (_isCrouchInputToggled.HasValue) {
                    _characterAccess.Input.CrouchToggled -= OnCrouchToggled;
                }
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
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            if (_characterAccess == null) {
                return _isCrouchInputActive.IsEmptyOrEquals(false) &&
                       _isCrouchInputPressed.IsEmptyOrEquals(false) &&
                       _isCrouchInputReleased.IsEmptyOrEquals(false) &&
                       _isCrouchInputToggled.IsEmptyOrEquals(false);
            }

            return _isCrouchInputActive.IsEmptyOrEquals(_characterAccess.Input.IsCrouchInputActive) &&
                   _isCrouchInputPressed.IsEmptyOrEquals(_characterAccess.Input.WasCrouchPressed) &&
                   _isCrouchInputReleased.IsEmptyOrEquals(_characterAccess.Input.WasCrouchReleased) &&
                   _isCrouchInputToggled.IsEmptyOrEquals(_characterAccess.Input.WasCrouchToggled);
        }
    }

}
