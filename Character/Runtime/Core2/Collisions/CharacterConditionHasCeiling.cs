using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Core2.Collisions {

    [Serializable]
    public sealed class CharacterConditionHasCeiling : ICondition, IDynamicDataHost {

        public bool hasCeiling;

        public bool IsMatched => hasCeiling == _characterAccess.CeilingDetector.CollisionInfo.hasContact;

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
            if (IsMatched) _callback?.OnConditionMatch();

            _characterAccess.CeilingDetector.OnContact += OnContact;
            _characterAccess.CeilingDetector.OnLostContact += OnLostContact;
        }
        
        public void Disarm() {
            _characterAccess.CeilingDetector.OnContact -= OnContact;
            _characterAccess.CeilingDetector.OnLostContact -= OnLostContact;
            
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
