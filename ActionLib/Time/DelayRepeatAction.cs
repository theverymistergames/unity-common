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
    public sealed class DelayRepeatAction : IActorAction {

        [Min(0f)] public float delayFrom = 0f;
        [Min(0f)] public float delayTo = 1f;
        [SerializeReference] [SubclassSelector] public IActorAction action;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            while (!cancellationToken.IsCancellationRequested) {
                float delay = Random.Range(delayFrom, delayTo);

                if (delay > 0f) {
                    await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                        .SuppressCancellationThrow();
                }
                
                if (cancellationToken.IsCancellationRequested) return;
                
                if (action != null) await action.Apply(context, cancellationToken);
            }
        }
    }
    
}