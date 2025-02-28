using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.ActionLib.Time {
    
    [Serializable]
    public sealed class DelayFramesAction : IActorAction {

        [Min(0)] public int frames;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            int f = 0;
            
            while (f++ < frames && !cancellationToken.IsCancellationRequested) {
                await UniTask.Yield();
            }
        }
    }
    
}