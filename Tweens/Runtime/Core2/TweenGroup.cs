using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Tweens.Core2 {

    [Serializable]
    public sealed class TweenGroup : ITween {

        public Mode mode;
        [SerializeReference] [SubclassSelector] public List<ITween> tweens;

        public enum Mode {
            Sequential,
            Parallel,
        }

        private readonly List<float> _durationCache = new List<float>();

        public float CreateDuration() {
            return mode switch {
                Mode.Sequential => GetDurationSequential(),
                Mode.Parallel => GetDurationParallel(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public UniTask Play(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            if (tweens is not { Count: > 0 }) return default;

            int count = tweens.Count;
            if (count == 1) return tweens[0]?.Play(duration, startProgress, speed, cancellationToken) ?? default;

            return mode switch {
                Mode.Sequential => PlaySequential(duration, startProgress, speed, cancellationToken),
                Mode.Parallel => PlayParallel(duration, startProgress, speed, cancellationToken),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private async UniTask PlaySequential(
            float duration,
            float startProgress,
            float speed,
            CancellationToken cancellationToken
        ) {
            int count = tweens.Count;
            int startIndex = -1;
            float startTime = Mathf.Clamp01(startProgress) * duration;
            float durationSum = 0f;

            // Find start position by duration
            for (int i = 0; i < count; i++) {
                float localDuration = _durationCache[i];
                float localStartTime = durationSum;
                durationSum += localDuration;

                if (localStartTime <= startTime && startTime <= durationSum) {
                    startIndex = i;
                    startProgress = localDuration > 0f
                        ? Mathf.Clamp01((startTime - localStartTime) / localDuration)
                        : speed > 0f ? 1f : 0f;
                    break;
                }
            }

            if (startIndex < 0) return;

            // Play first tween with calculated start progress
            await tweens[startIndex].Play(_durationCache[startIndex], startProgress, speed, cancellationToken);

            // Setup start progress for next tweens
            startProgress = speed > 0f ? 0f : 1f;

            for (int i = startIndex + 1; i < count; i++) {
                if (cancellationToken.IsCancellationRequested) break;

                await tweens[i].Play(_durationCache[i], startProgress, speed, cancellationToken);
            }
        }

        private async UniTask PlayParallel(
            float duration,
            float startProgress,
            float speed,
            CancellationToken cancellationToken
        ) {
            int count = tweens.Count;
            float startTime = Mathf.Clamp01(startProgress) * duration;
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);

            for (int i = 0; i < count; i++) {
                float localDuration = _durationCache[i];
                float localProgress = localDuration <= 0f ? speed > 0f ? 1f : 0f : Mathf.Clamp01(startTime / localDuration);

                tasks[i] = tweens[i].Play(localDuration, localProgress, speed, cancellationToken);
            }

            await UniTask.WhenAll(tasks);

            ArrayPool<UniTask>.Shared.Return(tasks);
        }

        private float GetDurationSequential() {
            _durationCache.Clear();

            int count = tweens.Count;
            float sum = 0f;

            for (int i = 0; i < count; i++) {
                float duration = Mathf.Max(tweens[i]?.CreateDuration() ?? 0f, 0f);
                sum += duration;
                _durationCache.Add(duration);
            }

            return sum;
        }

        private float GetDurationParallel() {
            _durationCache.Clear();

            int count = tweens.Count;
            float max = 0f;

            for (int i = 0; i < count; i++) {
                float duration = Mathf.Max(tweens[i]?.CreateDuration() ?? 0f, 0f);
                if (duration > max) max = duration;
                _durationCache.Add(duration);
            }

            return max;
        }
    }

}
