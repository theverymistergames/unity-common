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

        private ICharacterInput _input;
        private IConditionCallback _callback;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _input = provider.GetData<CharacterAccess>().Input;
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            if (_isCrouchInputActive.HasValue || _isCrouchInputPressed.HasValue) {
                _input.CrouchPressed -= OnCrouchPressed;
                _input.CrouchPressed += OnCrouchPressed;
            }

            if (_isCrouchInputActive.HasValue || _isCrouchInputReleased.HasValue) {
                _input.CrouchReleased -= OnCrouchReleased;
                _input.CrouchReleased += OnCrouchReleased;
            }

            if (_isCrouchInputToggled.HasValue) {
                _input.CrouchToggled -= OnCrouchToggled;
                _input.CrouchToggled += OnCrouchToggled;
            }
        }

        public void Disarm() {
            if (_isCrouchInputActive.HasValue || _isCrouchInputPressed.HasValue) _input.CrouchPressed -= OnCrouchPressed;
            if (_isCrouchInputActive.HasValue || _isCrouchInputReleased.HasValue) _input.CrouchReleased -= OnCrouchReleased;
            if (_isCrouchInputToggled.HasValue) _input.CrouchToggled -= OnCrouchToggled;

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
            return _isCrouchInputActive.IsEmptyOrEquals(_input.IsCrouchInputActive) &&
                   _isCrouchInputPressed.IsEmptyOrEquals(_input.WasCrouchPressed) &&
                   _isCrouchInputReleased.IsEmptyOrEquals(_input.WasCrouchReleased) &&
                   _isCrouchInputToggled.IsEmptyOrEquals(_input.WasCrouchToggled);
        }
    }

}
