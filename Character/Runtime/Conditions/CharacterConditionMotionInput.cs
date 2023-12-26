using System;
using MisterGames.Character.Core;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionMotionInput : ICharacterCondition {

        public Optional<bool> isMotionInputActive;
        public Optional<bool> isMovingForward;

        public bool IsMatch(ICharacterAccess context) {
            var motionInput = context.GetPipeline<ICharacterMotionPipeline>().MotionInput;

            return isMotionInputActive.IsEmptyOrEquals(!motionInput.IsNearlyZero()) &&
                   isMovingForward.IsEmptyOrEquals(motionInput.y > 0f);
        }
    }

}
