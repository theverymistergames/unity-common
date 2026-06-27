using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterConditionMotionInput : IActorCondition {

        public Optional<bool> isMotionInputActive;
        public Optional<bool> isMovingForward;

        public bool IsMatch(IActor context) {
            var motionInput = context.GetComponent<CharacterMotionPipeline>().Input;

            return isMotionInputActive.IsEmptyOrEquals(motionInput != Vector2.zero) &&
                   isMovingForward.IsEmptyOrEquals(motionInput.y > 0f);
        }
    }

}
