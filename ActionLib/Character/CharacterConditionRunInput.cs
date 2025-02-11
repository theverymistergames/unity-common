﻿using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Input;
using MisterGames.Common.Data;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterConditionRunInput : IActorCondition {

        public Optional<bool> isRunInputPressed;

        public bool IsMatch(IActor context, float startTime) {
            return isRunInputPressed.IsEmptyOrEquals(context.GetComponent<CharacterInputPipeline>().IsRunPressed);
        }
    }

}
