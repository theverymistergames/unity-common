using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Collisions;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterConditionIsGrounded : IActorCondition {

        public bool isGrounded;

        public bool IsMatch(IActor context) {
            var groundDetector = context.GetComponent<CharacterCollisionPipeline>().GroundDetector;
            return groundDetector.CollisionInfo.hasContact == isGrounded;
        }
    }

}
