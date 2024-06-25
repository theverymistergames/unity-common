using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Tweens;

namespace MisterGames.ActionLib.Tweens {
    
    [Serializable]
    public sealed class StopTweenAction : IActorAction {
        
        public TweenRunner tweenRunner;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            tweenRunner.TweenPlayer.Stop();
            return default;
        }
    }
    
}