using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.GameObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.ActionLib.GameObjects {

    [Serializable]
    public sealed class EnableObjectAction : IActorAction {

        public bool enabled;
        [Min(0f)] public float delay;
        public Object[] objects;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (delay > 0f) {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
            }
            
            if (cancellationToken.IsCancellationRequested) return;
            
            for (int i = 0; i < objects.Length; i++) {
                objects[i].SetEnabled(enabled);
            }
        }
    }
    
}