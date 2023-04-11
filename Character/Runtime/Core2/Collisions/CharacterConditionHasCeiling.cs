using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Core2.Collisions {

    [Serializable]
    public sealed class CharacterConditionHasCeiling : ICondition {

        public bool _hasCeiling;

        public bool IsMatched => _hasCeiling == CharacterAccessProvider.CharacterAccess.CeilingDetector.CollisionInfo.hasContact;

        private IConditionCallback _callback;

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            CharacterAccessProvider.CharacterAccess.CeilingDetector.OnContact += OnContact;
            CharacterAccessProvider.CharacterAccess.CeilingDetector.OnLostContact += OnLostContact;
        }
        
        public void Disarm() {
            CharacterAccessProvider.CharacterAccess.CeilingDetector.OnContact -= OnContact;
            CharacterAccessProvider.CharacterAccess.CeilingDetector.OnLostContact -= OnLostContact;
            
            _callback = null;
        }

        private void OnContact() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnLostContact() {
            if (IsMatched) _callback?.OnConditionMatch();
        }
    }

}
