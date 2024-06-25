using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Scenario.Events;

namespace MisterGames.ActionLib.Events {
    
    [Serializable]
    public sealed class EventRaisedCondition : IActorCondition {

        public EventReference eventReference;
        public Mode mode;
        [VisibleIf(nameof(mode), 0, CompareMode.Greater)] public int comparedValue;
        
        public enum Mode {
            RaisedOnce,
            Equal,
            Less,
            More,
            LessOrEqual,
            MoreOrEqual,
        } 
        
        public bool IsMatch(IActor context) {
            int count = eventReference.GetRaiseCount();

            return mode switch {
                Mode.RaisedOnce => count > 0,
                Mode.Equal => count == comparedValue,
                Mode.Less => count < comparedValue,
                Mode.More => count > comparedValue,
                Mode.LessOrEqual => count <= comparedValue,
                Mode.MoreOrEqual => count >= comparedValue,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
}