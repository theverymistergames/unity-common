using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class TweenGroup : TweenGroupBase<IActor, IActorTween>, IActorTween {

        public TweenEvent[] events;
        
        public override UniTask Play(
            IActor context,
            float duration,
            float startProgress,
            float speed,
            CancellationToken cancellationToken = default
        ) {
            TweenExtensions.Play(
                context,
                data: (events, cancellationToken),
                duration,
                progressCallback: (actor, data, p, oldP) => data.events.NotifyTweenEvents(actor, p, oldP, data.cancellationToken),
                progressModifier: null,
                startProgress, speed, cancellationToken: cancellationToken
            ).Forget();
            
            return TweenExtensions.PlayGroup(context, mode, tweens, duration, startProgress, speed, cancellationToken);
        }
    }
    
}
