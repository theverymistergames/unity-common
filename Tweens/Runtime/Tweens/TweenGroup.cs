using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Tweens {

    [Serializable]
    public sealed class TweenGroup : ITween {

        public Mode mode;
        [SerializeReference] [SubclassSelector] public List<ITween> tweens;

        public enum Mode {
            Sequential,
            Parallel,
        }

        private readonly List<float> _durations = new List<float>();

        public float CreateDuration() {
            _durations.Clear();
            int count = tweens.Count;
            float duration = 0f;

            for (int i = 0; i < count; i++) {
                float localDuration = TweenExtensions.CreateDuration(tweens[i], mode, ref duration);
                _durations.Add(localDuration);
            }

            return duration;
        }

        public UniTask Play(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return mode switch {
                Mode.Sequential => TweenExtensions.PlaySequential(tweens, _durations, duration, startProgress, speed, cancellationToken),
                Mode.Parallel => TweenExtensions.PlayParallel(tweens, _durations, duration, startProgress, speed, cancellationToken),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

}
