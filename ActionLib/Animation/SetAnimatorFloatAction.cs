using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Animation {
    
    [Serializable]
    public sealed class SetAnimatorFloatAction : IActorAction {

        public LabelValue parameter;
        public float value;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (!context.TryGetComponent(out Animator animator)) return default;

            animator.SetFloat(parameter.GetValue(), value);
            return default;
        }
    }
    
}