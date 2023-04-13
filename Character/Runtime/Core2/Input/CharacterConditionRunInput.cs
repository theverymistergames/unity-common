using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Core2.Input {

    [Serializable]
    public sealed class CharacterConditionRunInput : ICondition, IDynamicDataHost {

        public bool _isRunInputToggled;

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
            if (_characterAccess != null) _characterAccess.Input.RunToggled += OnRunToggled;

            if (IsMatched) _callback?.OnConditionMatch();
        }
        
        public void Disarm() {
            if (_characterAccess != null) _characterAccess.Input.RunToggled -= OnRunToggled;

            _callback = null;
        }

        private void OnRunToggled() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            if (_characterAccess == null) return false;

            return _isRunInputToggled == _characterAccess.Input.WasRunToggled;
        }
    }

}
