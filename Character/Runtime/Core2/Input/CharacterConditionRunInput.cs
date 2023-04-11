using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Core2.Input {

    [Serializable]
    public sealed class CharacterConditionRunInput : ICondition {

        public bool _isRunInputToggled;

        public bool IsMatched => _isRunInputToggled == _wasRunInputToggled;

        private IConditionCallback _callback;
        private bool _wasRunInputToggled;

        public void Arm(IConditionCallback callback) {
            _callback = callback;
            
            CharacterAccessProvider.CharacterAccess.Input.RunToggled += OnRunToggled;
        }
        
        public void Disarm() {
            CharacterAccessProvider.CharacterAccess.Input.RunToggled -= OnRunToggled;
            
            _callback = null;
        }

        private void OnRunToggled() {
            _wasRunInputToggled = true;
            if (IsMatched) _callback?.OnConditionMatch();
            _wasRunInputToggled = false;
        }
    }

}
