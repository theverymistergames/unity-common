using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Time {
    
    [Serializable]
    public sealed class DelayRepeatAction : IActorAction {

        [Min(0f)] public float delayFrom = 0f;
        [Min(0f)] public float delayTo = 1f;
        public bool useUnscaledTime;
        [Min(-1)] public int maxTimes = -1;
        public bool needDelayBeforeFirstTime;
        [SerializeReference] [SubclassSelector] public IActorAction action;

        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            int count = 0;
            
            float delay = Random.Range(delayFrom, delayTo);

            if (delay > 0f && needDelayBeforeFirstTime) {
                await AsyncExt.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: useUnscaledTime, cancellationToken);
            }
            
            while ((count++ < maxTimes || maxTimes < 0) && !cancellationToken.IsCancellationRequested) {
                if (action != null) {
                    await action.Apply(context, cancellationToken);
                    if (cancellationToken.IsCancellationRequested || count >= maxTimes && maxTimes >= 0) return;
                }
                
                delay = Random.Range(delayFrom, delayTo);

                if (delay > 0f) {
                    await AsyncExt.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: useUnscaledTime, cancellationToken);
                }
            }
        }
    }
    
}