using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Actors.Actions {

    [Serializable]
    public sealed class IfAction : IActorAction {

        [SerializeReference] [SubclassSelector] public IActorCondition condition;
        [SerializeReference] [SubclassSelector] public IActorAction actionOnTrue;
        [SerializeReference] [SubclassSelector] public IActorAction actionOnFalse;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var nextAction = (condition?.IsMatch(context) ?? false) ? actionOnTrue : actionOnFalse;
            return nextAction?.Apply(context, cancellationToken) ?? default;
        }
    }
}
