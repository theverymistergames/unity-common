using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.ActionLib.Events {
    
    [Serializable]
    public sealed class RaiseEventAction : IActorAction {

        public EventReference eventReference;
        public Mode mode;
        [Min(0)] public int count = 1;

        public enum Mode {
            Raise,
            Set,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            switch (mode) {
                case Mode.Raise:
                    eventReference.Raise(count);
                    break;
                
                case Mode.Set:
                    eventReference.SetCount(count);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return default;
        }
    }
    
}