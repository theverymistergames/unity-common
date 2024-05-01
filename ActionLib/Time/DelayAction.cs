using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.ActionLib.Time {
    
    [Serializable]
    public sealed class DelayAction : IActorAction {

        [Min(0f)] public float delay;
        public Mode mode;
        [SerializeReference] [SubclassSelector] public IActorAction action;

        public enum Mode {
            WaitDelayAndAction,
            WaitDelayOnly,
            DontWait
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (mode != Mode.DontWait) return ApplyDelayed(context, cancellationToken);
            
            ApplyDelayed(context, cancellationToken).Forget();
            return default;
        }

        private async UniTask ApplyDelayed(IActor context, CancellationToken cancellationToken) {
            if (delay > 0f) {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();    
            }

            if (action == null) return;
            
            if (mode == Mode.WaitDelayAndAction) {
                await action.Apply(context, cancellationToken);
            }
            else {
                action.Apply(context, cancellationToken).Forget();
            }
        }
    }
    
}