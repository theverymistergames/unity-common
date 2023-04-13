using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Access;
using MisterGames.Collisions.Core;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Core2.Collisions {

    [Serializable]
    public sealed class CharacterConditionHasCeiling : ICondition, IDynamicDataHost {

        public bool hasCeiling;

        public bool IsMatched => CheckCondition();

        private ICollisionDetector _ceilingDetector;
        private IConditionCallback _callback;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _ceilingDetector = provider.GetData<CharacterAccess>().CeilingDetector;
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            _ceilingDetector.OnContact -= OnContact;
            _ceilingDetector.OnContact += OnContact;

            _ceilingDetector.OnLostContact -= OnLostContact;
            _ceilingDetector.OnLostContact += OnLostContact;

            if (IsMatched) _callback?.OnConditionMatch();
        }
        
        public void Disarm() {
            _ceilingDetector.OnContact -= OnContact;
            _ceilingDetector.OnLostContact -= OnLostContact;

            _callback = null;
        }

        private void OnContact() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnLostContact() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            return hasCeiling == _ceilingDetector.CollisionInfo.hasContact;
        }
    }

}
