using System;
using System.Collections.Generic;
using MisterGames.Character.Core;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;

namespace MisterGames.Character.Input {

    [Serializable]
    public sealed class CharacterConditionRunInput : ICondition, IDynamicDataHost {

        public Optional<bool> isRunInputActive;
        public Optional<bool> isRunInputPressed;
        public Optional<bool> isRunInputReleased;

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

            _input.RunPressed -= OnRunPressed;
            _input.RunPressed += OnRunPressed;

            _input.RunReleased -= OnRunReleased;
            _input.RunReleased += OnRunReleased;
        }

        public void Disarm() {
            _input.RunPressed -= OnRunPressed;
            _input.RunReleased -= OnRunReleased;

            _callback = null;
        }

        public void OnFired() { }

        private void OnRunPressed() {
            if (IsMatched) _callback?.OnConditionMatch(this);
        }

        private void OnRunReleased() {
            if (IsMatched) _callback?.OnConditionMatch(this);
        }

        private bool CheckCondition() {
            return isRunInputActive.IsEmptyOrEquals(_input.IsRunPressed) &&
                   isRunInputPressed.IsEmptyOrEquals(_input.WasRunPressed) &&
                   isRunInputReleased.IsEmptyOrEquals(_input.WasRunReleased);
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionRunInput)}(" +
                   $"active {isRunInputActive}, " +
                   $"pressed {isRunInputPressed}, " +
                   $"released {isRunInputReleased})";
        }
    }

}
