using System;
using System.Collections.Generic;
using MisterGames.Character.Core;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;

namespace MisterGames.Character.Input {

    [Serializable]
    public sealed class CharacterConditionCrouchInput : ICondition, IDynamicDataHost {

        public Optional<bool> isCrouchInputActive;
        public Optional<bool> isCrouchInputPressed;
        public Optional<bool> isCrouchInputReleased;
        public Optional<bool> isCrouchInputToggled;

        public bool IsMatched => CheckCondition();

        private ICharacterInputPipeline _input;
        private IConditionCallback _callback;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _input = provider.GetData<CharacterAccess>().GetPipeline<ICharacterInputPipeline>();
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            if (isCrouchInputActive.HasValue || isCrouchInputPressed.HasValue) {
                _input.CrouchPressed -= OnCrouchPressed;
                _input.CrouchPressed += OnCrouchPressed;
            }

            if (isCrouchInputActive.HasValue || isCrouchInputReleased.HasValue) {
                _input.CrouchReleased -= OnCrouchReleased;
                _input.CrouchReleased += OnCrouchReleased;
            }

            if (isCrouchInputToggled.HasValue) {
                _input.CrouchToggled -= OnCrouchToggled;
                _input.CrouchToggled += OnCrouchToggled;
            }
        }

        public void Disarm() {
            if (isCrouchInputActive.HasValue || isCrouchInputPressed.HasValue) _input.CrouchPressed -= OnCrouchPressed;
            if (isCrouchInputActive.HasValue || isCrouchInputReleased.HasValue) _input.CrouchReleased -= OnCrouchReleased;
            if (isCrouchInputToggled.HasValue) _input.CrouchToggled -= OnCrouchToggled;

            _callback = null;
        }

        public void OnFired() { }

        private void OnCrouchPressed() {
            if (IsMatched) _callback?.OnConditionMatch(this);
        }

        private void OnCrouchReleased() {
            if (IsMatched) _callback?.OnConditionMatch(this);
        }

        private void OnCrouchToggled() {
            if (IsMatched) _callback?.OnConditionMatch(this);
        }

        private bool CheckCondition() {
            return isCrouchInputActive.IsEmptyOrEquals(_input.IsCrouchPressed) &&
                   isCrouchInputPressed.IsEmptyOrEquals(_input.WasCrouchPressed) &&
                   isCrouchInputReleased.IsEmptyOrEquals(_input.WasCrouchReleased) &&
                   isCrouchInputToggled.IsEmptyOrEquals(_input.WasCrouchToggled);
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionCrouchInput)}(" +
                   $"active {isCrouchInputActive}, " +
                   $"pressed {isCrouchInputPressed}, " +
                   $"released {isCrouchInputReleased}, " +
                   $"toggled {isCrouchInputToggled})";
        }
    }

}
