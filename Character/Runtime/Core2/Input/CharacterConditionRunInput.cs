using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Core2.Input {

    [Serializable]
    public sealed class CharacterConditionRunInput : ICondition, IDynamicDataHost {

        public bool _isRunInputToggled;

        public bool IsMatched => _isRunInputToggled == _wasRunInputToggled;

        private ICharacterAccess _characterAccess;
        private IConditionCallback _callback;
        private bool _wasRunInputToggled;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _characterAccess = provider.GetData<CharacterAccess>();
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;
            
            _characterAccess.Input.RunToggled += OnRunToggled;
        }
        
        public void Disarm() {
            _characterAccess.Input.RunToggled -= OnRunToggled;
            
            _callback = null;
        }

        private void OnRunToggled() {
            _wasRunInputToggled = true;
            if (IsMatched) _callback?.OnConditionMatch();
            _wasRunInputToggled = false;
        }
    }

}
