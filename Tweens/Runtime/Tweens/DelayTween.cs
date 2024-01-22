using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Tick.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class DelayTween : ITween {

        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandomAdd;

        public float CreateDuration() {
            return duration + Random.Range(-durationRandomAdd, durationRandomAdd);
        }

        public UniTask Play(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return TweenExtensions.Play(
                this,
                duration,
                progressCallback: null,
                startProgress,
                speed,
                PlayerLoopStage.Update,
                cancellationToken
            );
        }
    }

}
