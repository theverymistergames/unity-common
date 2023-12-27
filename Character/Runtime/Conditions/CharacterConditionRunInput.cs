using System;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionRunInput : ICharacterCondition {

        public Optional<bool> isRunInputPressed;

        public bool IsMatch(ICharacterAccess context) {
            var input = context.GetPipeline<ICharacterInputPipeline>();
            return isRunInputPressed.IsEmptyOrEquals(input.IsRunPressed);
        }
    }

}
