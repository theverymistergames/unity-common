using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints.Runtime;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.BlueprintLib.Tweens {

    public static class BlueprintTweenHelper {

        public static float CreateDurationFromLinkedTweens(
            LinkIterator links,
            TweenGroup.Mode mode,
            List<(ITween tween, float duration)> dest
        ) {
            float duration = 0f;

            while (links.MoveNext()) {
                if (links.Read<ITween>() is { } t) {
                    dest.Add((t, TweenExtensions.CreateDuration(t, mode, ref duration)));
                    continue;
                }

                if (links.Read<ITween[]>() is { } array) {
                    for (int i = 0; i < array.Length; i++) {
                        if (array[i] is { } t1) {
                            dest.Add((t1, TweenExtensions.CreateDuration(t1, mode, ref duration)));
                        }
                    }
                }
            }

            return duration;
        }

        public static async UniTask PlayTwoTweensAsSequence<T>(
            T data,
            CreateTween<T> firstTask,
            CreateTween<T> secondTask,
            float firstDuration,
            float totalDuration,
            float startProgress,
            float speed,
            CancellationToken cancellationToken = default
        ) {
            float sourceProgress = speed > 0f ? 0f : 1f;
            float targetProgress = speed > 0f ? 1f : 0f;
            float startTime = startProgress * totalDuration;
            float nextDuration = totalDuration - firstDuration;

            if (startTime >= firstDuration) {
                float localProgress = nextDuration > 0f ? (startTime - firstDuration) / nextDuration : targetProgress;
                if (secondTask != null) await secondTask.Invoke(data, nextDuration, localProgress, speed, cancellationToken);

                if (!cancellationToken.IsCancellationRequested && speed <= 0f) {
                    if (firstTask != null) await firstTask.Invoke(data, firstDuration, sourceProgress, speed, cancellationToken);
                }

                return;
            }

            float selfStartProgress = firstDuration > 0f ? Mathf.Clamp01(startTime / firstDuration) : targetProgress;
            if (firstTask != null) await firstTask.Invoke(data, firstDuration, selfStartProgress, speed, cancellationToken);

            if (!cancellationToken.IsCancellationRequested && speed >= 0f) {
                if (secondTask != null) await secondTask.Invoke(data, nextDuration, sourceProgress, speed, cancellationToken);
            }
        }
    }

}
