using System;
using System.Collections.Generic;
using MisterGames.Character.Access;
using MisterGames.Collisions.Core;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    [Serializable]
    public sealed class CharacterConditionHasCeiling : ICondition, IDynamicDataHost {

        public bool hasCeiling;
        public Optional<float> minCeilingHeight;

        public bool IsMatched => CheckCondition();

        private ICollisionDetector _ceilingDetector;
        private ITransformAdapter _bodyAdapter;
        private CharacterController _characterController;

        private IConditionCallback _callback;

        public void OnSetDataTypes(HashSet<Type> types) {
            types.Add(typeof(CharacterAccess));
        }

        public void OnSetData(IDynamicDataProvider provider) {
            var characterAccess = provider.GetData<CharacterAccess>();

            _ceilingDetector = characterAccess.CeilingDetector;
            _bodyAdapter = characterAccess.BodyAdapter;
            _characterController = characterAccess.CharacterController;
        }

        public void Arm(IConditionCallback callback) {
            _callback = callback;

            _ceilingDetector.OnContact -= OnContact;
            _ceilingDetector.OnContact += OnContact;

            _ceilingDetector.OnLostContact -= OnLostContact;
            _ceilingDetector.OnLostContact += OnLostContact;
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
            var info = _ceilingDetector.CollisionInfo;

            // No contact:
            // return true if no contact is expected (hasCeiling == false)
            if (!info.hasContact) return !hasCeiling;

            // Has contact, no ceiling height limit (considering has contact):
            // return true if contact is expected (hasCeiling == true)
            if (!minCeilingHeight.HasValue) return hasCeiling;

            float radius = _characterController.radius;
            var bottomPoint = _bodyAdapter.Position +
                              _characterController.center +
                              ((_characterController.height - radius) * 0.5f + radius) * Vector3.down;

            float sqrCeilingHeight = (info.lastHitPoint - bottomPoint).sqrMagnitude;

            // Has contact, current ceiling height is above or equal min limit (considering has no contact):
            // return true if no contact is expected (hasCeiling == false)
            if (sqrCeilingHeight >= minCeilingHeight.Value * minCeilingHeight.Value) return !hasCeiling;

            // Has contact, current ceiling height is below min limit (considering has contact):
            // return true if contact is expected (hasCeiling == true)
            return hasCeiling;
        }
    }

}
