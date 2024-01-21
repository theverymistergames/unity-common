using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using MisterGames.Tweens.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Tweens.Core2 {

    [Serializable]
    public sealed class ProgressTween : ITween {

        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandomAdd;
        public AnimationCurve curve;
        [SerializeReference] [SubclassSelector] public ITweenProgressCallback action;

        public float CreateDuration() {
            return duration + Random.Range(-durationRandomAdd, durationRandomAdd);
        }

        public UniTask Play(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return TweenExtensions.Play(
                this,
                duration,
                progressCallback: (t, p) => t.action?.OnProgressUpdate(t.curve?.Evaluate(p) ?? 0f),
                startProgress,
                speed,
                PlayerLoopStage.Update,
                cancellationToken
            );
        }
    }

}
