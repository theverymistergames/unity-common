using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Capsule;
using MisterGames.Character.Phys;
using MisterGames.Character.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterConditionHasCeiling : IActorCondition {

        public bool hasCeiling;
        public Optional<float> minCeilingHeight;

        public bool IsMatch(IActor context, float startTime) {
            var ceilingDetector = context.GetComponent<CharacterCeilingDetector>();
            var info = ceilingDetector.CollisionInfo;

            // No contact:
            // return true if no contact is expected
            if (!info.hasContact) return !hasCeiling;

            // Has contact, no ceiling height limit:
            // return true if contact is expected
            if (!minCeilingHeight.HasValue) return hasCeiling;

            var capsule = context.GetComponent<CharacterCapsulePipeline>();
            var top = capsule.GetColliderTopPoint();
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
