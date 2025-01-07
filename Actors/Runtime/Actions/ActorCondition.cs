using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Actors.Actions {

    [CreateAssetMenu(fileName = nameof(ActorCondition), menuName = "MisterGames/Actors/" + nameof(ActorCondition))]
    public sealed class ActorCondition : ScriptableObject, IActorCondition {

        [SerializeReference] [SubclassSelector] private IActorCondition _condition;

        public bool IsMatch(IActor context, float startTime) {
            return _condition.IsMatch(context, startTime);
        }
    }

}
