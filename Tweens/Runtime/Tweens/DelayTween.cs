using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class DelayTween : IActorTween {

        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandomAdd;
        public TweenEvent[] events;
        
        public float Duration { get; private set; }

        public void CreateNextDuration() {
            Duration = duration + Random.Range(-durationRandomAdd, durationRandomAdd);
        }

        public UniTask Play(IActor context, float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return TweenExtensions.Play(
                context,
                data: (self: this, token: cancellationToken),
                duration,
                progressCallback: (actor, data, p, oldP) => data.self.events.NotifyTweenEvents(actor, p, oldP, data.token),
                progressModifier: null,
                startProgress,
                speed,
                cancellationToken
            );
        }
    }

}
