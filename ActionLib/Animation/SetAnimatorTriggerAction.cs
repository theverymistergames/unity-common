using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.ActionLib.Animation {
    
    [Serializable]
    public sealed class SetAnimatorTriggerAction : IActorAction {

        public LabelValue parameter;
        public Operation operation;

        public enum Operation {
            Set,
            Reset,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (!context.TryGetComponent(out Animator animator)) return default;

            switch (operation) {
                case Operation.Set:
                    animator.SetTrigger(parameter.GetValue());
                    break;
                
                case Operation.Reset:
                    animator.ResetTrigger(parameter.GetValue());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return default;
        }
    }
    
}