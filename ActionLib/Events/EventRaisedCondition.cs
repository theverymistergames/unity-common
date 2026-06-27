using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using MisterGames.Scenario.Events;

namespace MisterGames.ActionLib.Events {
    
    [Serializable]
    public sealed class EventRaisedCondition : IActorCondition {

        public EventReference eventReference;
        public CompareMode compareMode;
        public int comparedValue;
        
        public bool IsMatch(IActor context) {
            return compareMode.IsMatch(eventReference.GetCount(), comparedValue);
        }
    }
    
}