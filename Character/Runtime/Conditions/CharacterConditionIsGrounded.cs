using System;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionIsGrounded : ICharacterCondition {

        public bool isGrounded;

        public bool IsMatch(ICharacterAccess context) {
            var groundDetector = context.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;
            return groundDetector.CollisionInfo.hasContact == isGrounded;
        }
    }

}
