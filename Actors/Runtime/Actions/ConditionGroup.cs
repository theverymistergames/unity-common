using System;
using MisterGames.Common.Conditions;

namespace MisterGames.Actors.Actions {

    [Serializable]
    public sealed class ConditionGroup : ConditionGroup<IActorCondition, IActor>, IActorCondition { }

}
