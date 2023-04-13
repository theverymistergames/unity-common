﻿using System;
using System.Collections.Generic;
using MisterGames.Character.Core2.Access;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Core2.Collisions {

    [Serializable]
    public sealed class CharacterConditionIsGrounded : ICondition, IDynamicDataHost {

        public bool isGrounded;

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

            if (_characterAccess != null) {
                _characterAccess.GroundDetector.OnContact += OnContact;
                _characterAccess.GroundDetector.OnLostContact += OnLostContact;
            }

            if (IsMatched) _callback?.OnConditionMatch();
        }
        
        public void Disarm() {
            if (_characterAccess != null) {
                _characterAccess.GroundDetector.OnContact -= OnContact;
                _characterAccess.GroundDetector.OnLostContact -= OnLostContact;
            }

            _callback = null;
        }

        private void OnContact() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private void OnLostContact() {
            if (IsMatched) _callback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            if (_characterAccess == null) return !isGrounded;

            return isGrounded == _characterAccess.GroundDetector.CollisionInfo.hasContact;
        }
    }

}
