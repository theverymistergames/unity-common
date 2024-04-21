using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Actors.Actions {

    [CreateAssetMenu(fileName = nameof(ActorAction), menuName = "MisterGames/Actors/" + nameof(ActorAction))]
    public sealed class ActorAction : ScriptableObject, IActorAction {

        [SerializeReference] [SubclassSelector] private IActorAction _action;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return _action?.Apply(context, cancellationToken) ?? default;
        }
    }

}
