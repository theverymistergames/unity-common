using System;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Core2.Collisions {

    [Serializable]
    public sealed class CharacterConditionIsGrounded : ICondition {

        public bool _isGrounded;

        public bool IsMatched => _isGrounded == CharacterAccessProvider.CharacterAccess.GroundDetector.CollisionInfo.hasContact;

        private IConditionCallback _callback;

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            CharacterAccessProvider.CharacterAccess.GroundDetector.OnContact += OnContact;
            CharacterAccessProvider.CharacterAccess.GroundDetector.OnLostContact += OnLostContact;
        }
        
        public void Disarm() {
            CharacterAccessProvider.CharacterAccess.GroundDetector.OnContact -= OnContact;
            CharacterAccessProvider.CharacterAccess.GroundDetector.OnLostContact -= OnLostContact;

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
