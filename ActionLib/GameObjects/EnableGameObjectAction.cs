using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {

    [Serializable]
    public sealed class EnableGameObjectAction : IActorAction {

        public bool enabled;
        [Min(0f)] public float delay;
        public bool useUnscaledTime;
        public bool wait = true;
        public GameObject[] gameObjects;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (wait) return ApplyInternal(cancellationToken);

            ApplyInternal(cancellationToken).Forget();
            return UniTask.CompletedTask;
        }
        
        public async UniTask ApplyInternal(CancellationToken cancellationToken) {
            if (delay > 0f) {
                await AsyncExt.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: useUnscaledTime, cancellationToken);
            }
            
            if (cancellationToken.IsCancellationRequested) return;
            
            for (int i = 0; i < gameObjects.Length; i++) {
                gameObjects[i].SetActive(enabled);
            }
        }
    }
    
}