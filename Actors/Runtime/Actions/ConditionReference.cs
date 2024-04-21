using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class ConditionReference : IActorCondition {

        public ActorCondition condition;
        public bool defaultCondition;

        public bool IsMatch(IActor context) {
            return condition == null ? defaultCondition : condition.IsMatch(context);
        }
    }

}
