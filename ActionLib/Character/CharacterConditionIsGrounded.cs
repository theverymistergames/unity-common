using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Phys;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterConditionIsGrounded : IActorCondition {

        public bool isGrounded;

        public bool IsMatch(IActor context, float startTime) {
            return context.GetComponent<CharacterGroundDetector>().CollisionInfo.hasContact == isGrounded;
        }
    }

}
