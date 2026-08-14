using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.ActionLib.Flow {

    [Serializable]
    public sealed class WaitConditionAction : IActorAction {

        [Min(0f)] public float checkPeriod;
        [SerializeReference] [SubclassSelector] public IActorCondition condition;
        public bool useUnscaledTime;

        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            while (!cancellationToken.IsCancellationRequested) {
                if (condition.IsMatch(context)) return;

                if (checkPeriod > 0f) {
                    await AsyncExt.Delay(TimeSpan.FromSeconds(checkPeriod), ignoreTimeScale: useUnscaledTime, cancellationToken);
                    continue;
                }

                await UniTask.Yield();
            }
        }
    }
    
}