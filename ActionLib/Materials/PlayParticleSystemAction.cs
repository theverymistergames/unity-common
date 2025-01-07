using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.Materials {
    
    [Serializable]
    public sealed class PlayParticleSystemAction : IActorAction {

        public ParticleSystem particleSystem;
        public Operation operation;
        public bool withChildren = true;
        
        public enum Operation {
            Play,
            StopEmitting,
            StopEmittingAndClear,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            switch (operation) {
                case Operation.Play:
                    particleSystem.Play(withChildren);
                    break;
                
                case Operation.StopEmitting:
                    particleSystem.Stop(withChildren, ParticleSystemStopBehavior.StopEmitting);
                    break;
                
                case Operation.StopEmittingAndClear:
                    particleSystem.Stop(withChildren, ParticleSystemStopBehavior.StopEmittingAndClear);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }

            return default;
        }
    }
    
}