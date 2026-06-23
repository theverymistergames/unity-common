using System;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.Scenario.Service {
    
    [Serializable]
    public struct ScenarioEvent {
        public EventReference startEvent;
        public Optional<CompareInt> expectedCount;
        [SerializeReference] [SubclassSelector] public IActorAction startAction;
    }
    
}