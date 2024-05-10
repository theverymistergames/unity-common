using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterConditionMotionInput : IActorCondition {

        public Optional<bool> isMotionInputActive;
        public Optional<bool> isMovingForward;

        public bool IsMatch(IActor context) {
            var motionInput = context.GetComponent<CharacterMotionPipeline>().MotionInput;

            return isMotionInputActive.IsEmptyOrEquals(!motionInput.IsNearlyZero()) &&
                   isMovingForward.IsEmptyOrEquals(motionInput.y > 0f);
        }
    }

}
