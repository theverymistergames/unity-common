using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.ActionLib.Events {
    
    [Serializable]
    public sealed class ResetEventDomainAction : IActorAction {

        public EventDomain eventDomain;
        public bool includeSaved;
        public bool notify;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            EventBus.Main.ResetEventsOf(eventDomain, includeSaved, notify);
            return default;
        }
    }
    
}