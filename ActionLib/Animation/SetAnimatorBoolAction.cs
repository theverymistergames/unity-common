using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Animation {
    
    [Serializable]
    public sealed class SetAnimatorBoolAction : IActorAction {

        public HashId parameter;
        public bool value;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (!context.TryGetComponent(out Animator animator)) return default;

            animator.SetBool(parameter, value);
            return default;
        }
    }
    
}