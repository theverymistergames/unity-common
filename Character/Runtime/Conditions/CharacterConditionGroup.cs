using System;
using MisterGames.Character.Core;
using MisterGames.Common.Conditions;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionGroup : ConditionGroup<ICharacterAccess, ICharacterCondition> { }

}
