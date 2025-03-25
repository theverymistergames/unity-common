using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Animation {
    
    [Serializable]
    public sealed class SetAnimatorIntAction : IActorAction {

        public Mode mode;
        [VisibleIf(nameof(mode), 1)]
        public Animator animator;
        public HashId parameter;
        public int value;

        public enum Mode {
            Actor,
            Explicit,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var anim = mode switch {
                Mode.Actor => context.GetComponent<Animator>(),
                Mode.Explicit => animator,
                _ => throw new ArgumentOutOfRangeException()
            };

            anim.SetInteger(parameter, value);
            return default;
        }
    }
    
}