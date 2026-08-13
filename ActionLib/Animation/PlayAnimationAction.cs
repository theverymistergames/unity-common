using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Animation {
    
    [Serializable]
    public sealed class PlayAnimationAction : IActorAction {
        
        public Animator animator;
        public HashId state;
        [Min(-1)] public int layer = -1;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            animator.Play(state, layer);
            return default;
        }
    }
    
}