﻿using System;
using System.Collections.Generic;
using MisterGames.Character.Core;
using MisterGames.Collisions.Core;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Collisions {

    [Serializable]
    public sealed class CharacterConditionIsGrounded : ICondition, IDynamicDataHost {

        public bool isGrounded;

        public bool IsMatched => CheckCondition();

        private ICollisionDetector _groundDetector;
        private IConditionCallback _callback;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            _groundDetector = provider.GetData<CharacterAccess>().GetPipeline<ICharacterCollisionPipeline>().GroundDetector;
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            _groundDetector.OnContact -= OnContact;
            _groundDetector.OnContact += OnContact;

            _groundDetector.OnLostContact -= OnLostContact;
            _groundDetector.OnLostContact += OnLostContact;
        }
        
        public void Disarm() {
            _groundDetector.OnContact -= OnContact;
            _groundDetector.OnLostContact -= OnLostContact;

            _callback = null;
        }

        public void OnFired() { }

        private void OnContact() {
            if (IsMatched) _callback?.OnConditionMatch(this);
        }

        private void OnLostContact() {
            if (IsMatched) _callback?.OnConditionMatch(this);
        }

        private bool CheckCondition() {
            return isGrounded == _groundDetector.CollisionInfo.hasContact;
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionIsGrounded)}(isGrounded {isGrounded})";
        }
    }

}
