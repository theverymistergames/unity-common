using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.ActionLib.Phys {
    
    [Serializable]
    public sealed class SetKinematicAction : IActorAction {

        public bool isKinematic;
        public Mode mode;
        [VisibleIf(nameof(mode), 1)]
        public Rigidbody rigidbody;
        
        public enum Mode {
            Actor,
            Explicit,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var rb = mode switch {
                Mode.Actor => context.GetComponent<Rigidbody>(),
                Mode.Explicit => rigidbody,
                _ => throw new ArgumentOutOfRangeException()
            };
            rb.isKinematic = isKinematic;
            return default;
        }
    }
    
}