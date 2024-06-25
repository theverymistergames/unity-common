using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using MisterGames.Tweens;

namespace MisterGames.ActionLib.Tweens {
    
    [Serializable]
    public sealed class PlayTweenAction : IActorAction {
        
        public TweenRunner tweenRunner;
        public Optional<float> speed = new(value: 1f, hasValue: false);
        public Optional<float> progress = new(value: 0f, hasValue: false);
        public bool waitTween;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var tween = tweenRunner.TweenPlayer;
            
            if (speed.HasValue) tween.Speed = speed.Value;
            if (progress.HasValue) tween.Progress = progress.Value;

            if (waitTween) {
                return tween.Play(cancellationToken: cancellationToken);
            }

            tween.Play(cancellationToken: cancellationToken).Forget();
            return default;
        }
    }
    
}