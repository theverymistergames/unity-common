using System;
using MisterGames.Common.Conditions;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionGroup : ConditionGroup<(IInteractiveUser, IInteractive), IInteractCondition> { }

}
