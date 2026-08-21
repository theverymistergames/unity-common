using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.GameObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.ActionLib.GameObjects {

    [Serializable]
    public sealed class EnableObjectAction : IActorAction {

        public bool enabled;
        [Min(0f)] public float delay;
        public bool useUnscaledTime;
        public bool wait = true;
        public Object[] objects;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (wait) return ApplyInternal(cancellationToken);
            
            ApplyInternal(cancellationToken).Forget();
            return UniTask.CompletedTask;
        }
        
        private async UniTask ApplyInternal(CancellationToken cancellationToken = default) {
            if (delay > 0f) {
                await AsyncExt.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: useUnscaledTime, cancellationToken);
            }
            
            if (cancellationToken.IsCancellationRequested) return;
            
            for (int i = 0; i < objects.Length; i++) {
                objects[i].SetEnabled(enabled);
            }
        }
    }
    
}