using System;
using MisterGames.Character.Capsule;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Common.Data;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionHasCeiling : ICharacterCondition {

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
    }

}
