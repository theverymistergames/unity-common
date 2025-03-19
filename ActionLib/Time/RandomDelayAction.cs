using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Time {
    
    [Serializable]
    public sealed class RandomDelayAction : IActorAction {

        [Min(0)] public float delayFrom;
        [Min(0)] public float delayTo;
        public DelayAction.Mode mode;
        [SerializeReference] [SubclassSelector] public IActorAction action;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            float delay = Random.Range(delayFrom, delayTo);
            
            if (mode != DelayAction.Mode.DontWait) return ApplyDelayed(context, delay, cancellationToken);
            
            ApplyDelayed(context, delay, cancellationToken).Forget();
            return default;
        }

        private async UniTask ApplyDelayed(IActor context, float delay, CancellationToken cancellationToken) {
            if (delay > 0f) {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();    
            }

            if (cancellationToken.IsCancellationRequested || action == null) return;
            
            if (mode == DelayAction.Mode.WaitDelayAndAction) {
                await action.Apply(context, cancellationToken);
            }
            else {
                action.Apply(context, cancellationToken).Forget();
            }
        }
    }
    
}