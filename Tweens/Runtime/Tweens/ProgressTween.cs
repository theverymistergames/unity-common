using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Common.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class ProgressTween : IActorTween {

        [Min(0f)] public float duration = 1f;
        [Min(0f)] public float durationRandomAdd;
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeReference] [SubclassSelector] public IProgressModulator modulator;
        [SerializeReference] [SubclassSelector] public ITweenProgressAction action;
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
                progressCallback: (actor, data, p, oldP) => {
                    data.self.events.NotifyTweenEvents(actor, p, oldP, data.token);
                    data.self.action?.OnProgressUpdate(p);
                },
                progressModifier: (data, p) => {
                    p = data.self.curve?.Evaluate(p) ?? p;
                    return data.self.modulator?.Modulate(p) ?? p;
                },
                startProgress,
                speed,
                cancellationToken
            );
        }
    }

}
