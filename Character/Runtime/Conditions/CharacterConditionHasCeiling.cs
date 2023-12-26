using System;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Character.Capsule;
using MisterGames.Collisions.Core;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionHasCeiling : ICharacterCondition, ITransition, IDependency {

        public bool hasCeiling;
        public Optional<float> minCeilingHeight;

        public bool IsMatch(ICharacterAccess context) {
            var ceilingDetector = context.GetPipeline<ICharacterCollisionPipeline>().CeilingDetector;
            var info = ceilingDetector.CollisionInfo;

            // No contact:
            // return true if no contact is expected
            if (!info.hasContact) return !hasCeiling;

            // Has contact, no ceiling height limit:
            // return true if contact is expected
            if (!minCeilingHeight.HasValue) return hasCeiling;

            var capsule = context.GetPipeline<ICharacterCapsulePipeline>();
            var top = capsule.ColliderTop;
            float sqrDistanceToCeiling = (info.point - top).sqrMagnitude;

            // Has contact, current distance from character top point to ceiling contact point is above min limit:
            // return true if no contact is expected
            if (sqrDistanceToCeiling > minCeilingHeight.Value * minCeilingHeight.Value) return !hasCeiling;

            // Has contact, current distance from character top point to ceiling contact point is below min limit:
            // return true if contact is expected
            return hasCeiling;
        }

        public bool IsMatched => CheckCondition();

        private ICollisionDetector _ceilingDetector;
        private ITransformAdapter _bodyAdapter;
        private ICharacterCapsulePipeline _capsule;
        private ITransitionCallback _callback;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            var characterAccess = resolver.Resolve<ICharacterAccess>();

            _ceilingDetector = characterAccess.GetPipeline<ICharacterCollisionPipeline>().CeilingDetector;
            _bodyAdapter = characterAccess.BodyAdapter;
            _capsule = characterAccess.GetPipeline<ICharacterCapsulePipeline>();
        }

        public void Arm(ITransitionCallback callback) {
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

        public void OnFired() { }

        private void OnContact() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private void OnLostContact() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private bool CheckCondition() {
            var info = _ceilingDetector.CollisionInfo;

            // No contact:
            // return true if no contact is expected (hasCeiling == false)
            if (!info.hasContact) return !hasCeiling;

            // Has contact, no ceiling height limit (considering has contact):
            // return true if contact is expected (hasCeiling == true)
            if (!minCeilingHeight.HasValue) return hasCeiling;

            float radius = _capsule.Radius;
            var bottomPoint = _bodyAdapter.Position + _capsule.ColliderCenter +
                              ((_capsule.Height - radius) * 0.5f + radius) * Vector3.down;

            float sqrCeilingHeight = (info.point - bottomPoint).sqrMagnitude;

            // Has contact, current ceiling height is above or equal min limit (considering has no contact):
            // return true if no contact is expected (hasCeiling == false)
            if (sqrCeilingHeight >= minCeilingHeight.Value * minCeilingHeight.Value) return !hasCeiling;

            // Has contact, current ceiling height is below min limit (considering has contact):
            // return true if contact is expected (hasCeiling == true)
            return hasCeiling;
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionHasCeiling)}(hasCeiling {hasCeiling}, minHeight {minCeilingHeight})";
        }
    }

}
