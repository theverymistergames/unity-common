using System;
using System.Collections.Generic;
using MisterGames.Character.Access;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Input {

    [Serializable]
    public sealed class CharacterConditionRunInput : ICondition, IDynamicDataHost {

        public bool _isRunInputToggled;

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

            _input.RunToggled -= OnRunToggled;
            _input.RunToggled += OnRunToggled;
        }
        
        public void Disarm() {
            _input.RunToggled -= OnRunToggled;

            _callback = null;
        }

        private void OnRunToggled() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            return _isRunInputToggled == _input.WasRunToggled;
        }
    }

}
