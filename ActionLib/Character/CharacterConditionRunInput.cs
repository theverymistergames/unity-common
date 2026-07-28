using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Common.Data;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterConditionRunInput : IActorCondition {

        public Optional<bool> isRunInputPressed;

        public bool IsMatch(IActor context) {
            return isRunInputPressed.IsEmptyOrEquals(context.GetComponent<CharacterMotionRunPipeline>().IsRunActive);
        }
    }

}
