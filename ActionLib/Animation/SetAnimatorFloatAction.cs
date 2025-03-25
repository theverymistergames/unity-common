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
    public sealed class SetAnimatorFloatAction : IActorAction {

        public SetAnimatorIntAction.Mode mode;
        [VisibleIf(nameof(mode), 1)]
        public Animator animator;
        public HashId parameter;
        public float value;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var anim = mode switch {
                SetAnimatorIntAction.Mode.Actor => context.GetComponent<Animator>(),
                SetAnimatorIntAction.Mode.Explicit => animator,
                _ => throw new ArgumentOutOfRangeException()
            };

            anim.SetFloat(parameter, value);
            return default;
        }
    }
    
}