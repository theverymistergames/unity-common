using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Scenario.Events;

namespace MisterGames.ActionLib.Events {
    
    [Serializable]
    public sealed class ResetEventDomainAction : IActorAction {

        public EventDomain eventDomain;
        public bool notify;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            EventBus.Main.ResetEventsOf(eventDomain, notify);
            return default;
        }
    }
    
}