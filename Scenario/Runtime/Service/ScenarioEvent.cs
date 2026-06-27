using System;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.Scenario.Service {
    
    [Serializable]
    public sealed class ScenarioEvent {

        public string name;
        [TextAreaExtended]
        public string description;

        [Header("Event Conditions")]
        public EventReference startEvent;
        public Mode subIdMode;
        public Optional<CompareInt> expectedRaiseCount = Optional<CompareInt>.WithValue(new CompareInt { mode = CompareMode.Greater, value = 0 });
        [SerializeReference] [SubclassSelector] public IActorCondition condition;
        
        [Header("Action")]
        [SerializeReference] [SubclassSelector] public IActorAction startAction;

        public enum Mode {
            IgnoreSubId,
            ExpectSubId,
        }

        public override string ToString() {
            return $"ScenarioEvent(name {name}, startEvent {startEvent})";
        }
    }
    
}