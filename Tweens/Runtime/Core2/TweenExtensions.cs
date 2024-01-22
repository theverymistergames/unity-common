using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace MisterGames.Tweens.Core2 {

    public delegate void ProgressCallback(float progress);
    public delegate void ProgressCallback<in T>(T data, float progress);

    public delegate UniTask TweenFactory(float duration, float startProgress, float speed, CancellationToken cancellationToken = default);
    public delegate UniTask TweenFactory<in T>(T data, float duration, float startProgress, float speed, CancellationToken cancellationToken = default);

    public static class TweenExtensions {

        public static void PlayAndForget(
            float duration,
            ProgressCallback progressCallback = null,
            float startProgress = 0f,
            float speed = 1f,
            PlayerLoopStage playerLoopStage = PlayerLoopStage.Update,
            CancellationToken cancellationToken = default
        ) {
            Play(duration, progressCallback, startProgress, speed, playerLoopStage, cancellationToken).Forget();
        }

        public static void PlayAndForget<T>(
            T data,
            float duration,
            ProgressCallback<T> progressCallback = null,
            float startProgress = 0f,
            float speed = 1f,
            PlayerLoopStage playerLoopStage = PlayerLoopStage.Update,
            CancellationToken cancellationToken = default
        ) {
            Play(data, duration, progressCallback, startProgress, speed, playerLoopStage, cancellationToken).Forget();
        }

        public static async UniTask Play(
            float duration,
            ProgressCallback progressCallback = null,
            float startProgress = 0f,
            float speed = 1f,
            PlayerLoopStage playerLoopStage = PlayerLoopStage.Update,
            CancellationToken cancellationToken = default
        ) {
            if (cancellationToken.IsCancellationRequested) return;

            float progress = Mathf.Clamp01(startProgress);

            if (duration <= 0f) {
                float oldProgress = speed > 0f ? 0f : 1f;
                progress = speed > 0f ? 1f : 0f;
                if (!oldProgress.IsNearlyEqual(progress)) progressCallback?.Invoke(progress);
                return;
            }

            var timeSource = TimeSources.Get(playerLoopStage);

            while (!cancellationToken.IsCancellationRequested) {
                float oldProgress = progress;
                progress = Mathf.Clamp01(progress + timeSource.DeltaTime * speed / duration);
                if (!oldProgress.IsNearlyEqual(progress)) progressCallback?.Invoke(progress);

                if (speed > 0f && progress >= 1f || speed < 0f && progress <= 0f) {
                    break;
                }

                await UniTask.Yield();
            }
        }

        public static async UniTask Play<T>(
            T data,
            float duration,
            ProgressCallback<T> progressCallback = null,
            float startProgress = 0f,
            float speed = 1f,
            PlayerLoopStage playerLoopStage = PlayerLoopStage.Update,
            CancellationToken cancellationToken = default
        ) {
            if (cancellationToken.IsCancellationRequested) return;

            float progress = Mathf.Clamp01(startProgress);

            if (duration <= 0f) {
                float oldProgress = speed > 0f ? 0f : 1f;
                progress = speed > 0f ? 1f : 0f;
                if (!oldProgress.IsNearlyEqual(progress)) progressCallback?.Invoke(data, progress);
                return;
            }

            var timeSource = TimeSources.Get(playerLoopStage);

            while (!cancellationToken.IsCancellationRequested) {
                float oldProgress = progress;
                progress = Mathf.Clamp01(progress + timeSource.DeltaTime * speed / duration);
                if (!oldProgress.IsNearlyEqual(progress)) progressCallback?.Invoke(data, progress);

                if (speed > 0f && progress >= 1f || speed < 0f && progress <= 0f) {
                    break;
                }

                await UniTask.Yield();
            }
        }

        public static async UniTask PlaySequential(
            IReadOnlyList<ITween> tweens,
            IReadOnlyList<float> durations,
            float duration,
            float startProgress,
            float speed,
            CancellationToken cancellationToken
        ) {
            if (tweens is not { Count: > 0 }) return;
            int count = tweens.Count;

            if (count == 1) {
                if (tweens[0] is { } t) await t.Play(duration, startProgress, speed, cancellationToken);
                return;
            }

            int startIndex = -1;
            float startTime = Mathf.Clamp01(startProgress) * duration;
            float durationSum = 0f;
            float targetProgress = speed > 0f ? 1f : 0f;

            // Find start tween and its local start progress
            for (int i = 0; i < count; i++) {
                float localDuration = durations[i];
                float localStartTime = durationSum;
                durationSum += localDuration;

                if (localStartTime <= startTime && startTime <= durationSum) {
                    startIndex = i;
                    startProgress = localDuration > 0f
                        ? Mathf.Clamp01((startTime - localStartTime) / localDuration)
                        : targetProgress;

                    break;
                }
            }

            if (startIndex < 0) return;

            // Play first tween with calculated start progress
            if (tweens[startIndex] is {} t1) await t1.Play(durations[startIndex], startProgress, speed, cancellationToken);

            // Setup start progress for next tweens
            int dir = speed > 0f ? 1 : -1;
            startProgress = speed > 0f ? 0f : 1f;

            for (int i = startIndex + dir; i >= 0 && i < count; i += dir) {
                if (cancellationToken.IsCancellationRequested) break;

                if (tweens[i] is {} t2) await t2.Play(durations[i], startProgress, speed, cancellationToken);
            }
        }

        public static async UniTask PlaySequential(
            IReadOnlyList<(ITween tween, float duration)> tweens,
            float duration,
            float startProgress,
            float speed,
            CancellationToken cancellationToken
        ) {
            if (tweens is not { Count: > 0 }) return;
            int count = tweens.Count;

            if (count == 1) {
                if (tweens[0].tween is { } t) await t.Play(duration, startProgress, speed, cancellationToken);
                return;
            }

            int startIndex = -1;
            float startTime = Mathf.Clamp01(startProgress) * duration;
            float durationSum = 0f;
            float targetProgress = speed > 0f ? 1f : 0f;

            // Find start tween and its local start progress
            for (int i = 0; i < count; i++) {
                (_, float localDuration) = tweens[i];
                float localStartTime = durationSum;
                durationSum += localDuration;

                if (localStartTime <= startTime && startTime <= durationSum) {
                    startIndex = i;
                    startProgress = localDuration > 0f
                        ? Mathf.Clamp01((startTime - localStartTime) / localDuration)
                        : targetProgress;

                    break;
                }
            }

            if (startIndex < 0) return;

            // Play first tween with calculated start progress
            (var firstTween, float firstDuration) = tweens[startIndex];
            if (firstTween != null) await firstTween.Play(firstDuration, startProgress, speed, cancellationToken);

            // Setup start progress for next tweens
            int dir = speed > 0f ? 1 : -1;
            startProgress = speed > 0f ? 0f : 1f;

            for (int i = startIndex + dir; i >= 0 && i < count; i += dir) {
                if (cancellationToken.IsCancellationRequested) break;

                (var tween, float localDuration) = tweens[i];
                if (tween != null) await tween.Play(localDuration, startProgress, speed, cancellationToken);
            }
        }

        public static async UniTask PlayParallel(
            IReadOnlyList<ITween> tweens,
            IReadOnlyList<float> durations,
            float duration,
            float startProgress,
            float speed,
            CancellationToken cancellationToken
        ) {
            if (tweens is not { Count: > 0 }) return;
            int count = tweens.Count;

            if (count == 1) {
                if (tweens[0] is {} t) await t.Play(duration, startProgress, speed, cancellationToken);
                return;
            }

            float startTime = Mathf.Clamp01(startProgress) * duration;
            float targetProgress = speed > 0f ? 1f : 0f;
            float sourceProgress = speed > 0f ? 0f : 1f;
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);

            for (int i = 0; i < count; i++) {
                tasks[i] = UniTask.CompletedTask;
                float localDuration = Mathf.Max(0f, durations[i]);

                // Process tweens which should be delayed or skipped,
                // if local duration is less than start time.
                if (startTime > localDuration) {
                    // Skip if is playing forward
                    if (speed >= 0f) continue;

                    // Delay start if is playing backwards
                    float delay = Mathf.Max(startTime - localDuration, 0f);
                    tasks[i] = PlayDelayed(tweens[i], delay, localDuration, sourceProgress, speed, cancellationToken);

                    continue;
                }

                float localProgress = localDuration <= 0f ? targetProgress : Mathf.Clamp01(startTime / localDuration);
                tasks[i] = tweens[i]?.Play(localDuration, localProgress, speed, cancellationToken) ?? UniTask.CompletedTask;
            }

            await UniTask.WhenAll(tasks);

            ArrayPool<UniTask>.Shared.Return(tasks);
        }

        public static async UniTask PlayParallel(
            IReadOnlyList<(ITween tween, float duration)> tweens,
            float duration,
            float startProgress,
            float speed,
            CancellationToken cancellationToken
        ) {
            if (tweens is not { Count: > 0 }) return;
            int count = tweens.Count;

            if (count == 1) {
                if (tweens[0].tween is {} t) await t.Play(duration, startProgress, speed, cancellationToken);
                return;
            }

            float startTime = Mathf.Clamp01(startProgress) * duration;
            float targetProgress = speed > 0f ? 1f : 0f;
            float sourceProgress = speed > 0f ? 0f : 1f;
            var tasks = ArrayPool<UniTask>.Shared.Rent(count);

            for (int i = 0; i < count; i++) {
                tasks[i] = UniTask.CompletedTask;
                (var tween, float localDuration) = tweens[i];
                localDuration = Mathf.Max(0f, localDuration);

                // Process tweens which should be delayed or skipped,
                // if local duration is less than start time.
                if (startTime > localDuration) {
                    // Skip if is playing forward
                    if (speed >= 0f) continue;

                    // Delay start if is playing backwards
                    float delay = Mathf.Max(startTime - localDuration, 0f);
                    tasks[i] = PlayDelayed(tween, delay, localDuration, sourceProgress, speed, cancellationToken);

                    continue;
                }

                float localProgress = localDuration <= 0f ? targetProgress : Mathf.Clamp01(startTime / localDuration);
                tasks[i] = tween?.Play(localDuration, localProgress, speed, cancellationToken) ?? UniTask.CompletedTask;
            }

            await UniTask.WhenAll(tasks);

            ArrayPool<UniTask>.Shared.Return(tasks);
        }

        public static float CreateDuration(ITween tween, TweenGroup.Mode mode, ref float totalDuration) {
            float localDuration = Mathf.Max(tween?.CreateDuration() ?? 0f, 0f);

            switch (mode) {
                case TweenGroup.Mode.Sequential:
                    totalDuration += localDuration;
                    break;

                case TweenGroup.Mode.Parallel:
                    if (localDuration > totalDuration) totalDuration = localDuration;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return localDuration;
        }

        /// <summary>
        /// This is a builder for parallel tween group.
        /// Declare new variable of type ITween and assign it <c>null</c>,
        /// then pass it as parameter <paramref name="dest"/> by reference.
        /// Reference <paramref name="dest"/> will be assigned with tween implementations
        /// based on the amount of passed tweens to run in parallel.
        ///
        /// Usage:
        ///
        /// <code>
        /// ITween dest = null;
        /// TweenExtensions.MergeTweenIntoParallelGroup(ref dest, tween: new SomeTween()); // dest is SomeTween
        /// TweenExtensions.MergeTweenIntoParallelGroup(ref dest, tween: new SomeTween()); // dest is TweenGroup { tween, tween }
        /// </code>
        /// </summary>
        ///
        /// <param name="dest">Destination tween, is assigned as first passed tween, then as parallel tween group
        /// containing all passed tweens
        /// </param>
        /// <param name="tween">Tween to run in parallel</param>
        public static void MergeTweenIntoParallelGroup(ref ITween dest, ITween tween) {
            if (tween == null) return;

            if (dest == null) {
                dest = tween;
                return;
            }

            if (dest is TweenGroup { mode: TweenGroup.Mode.Parallel } tweenGroup) {
                MergeTweenIntoParallelGroup(tweenGroup, tween);
                return;
            }

            tweenGroup = new TweenGroup { mode = TweenGroup.Mode.Parallel };
            MergeTweenIntoParallelGroup(tweenGroup, tween);
            MergeTweenIntoParallelGroup(tweenGroup, dest);

            dest = tweenGroup;
        }

        private static void MergeTweenIntoParallelGroup(TweenGroup group, ITween tween) {
            Assert.IsTrue(group is { mode: TweenGroup.Mode.Parallel });

            if (tween == null) return;

            int lastGroupTweensCount = group.tweens?.Count ?? 0;

            if (group.tweens == null) group.tweens = new List<ITween>(1) { tween };
            else group.tweens.Add(tween);

            var groupTweens = group.tweens;

            // Iterate through added tweens to search for more parallel groups,
            // which can be flattened into root group.
            for (int i = lastGroupTweensCount; i < groupTweens.Count; i++) {
                var t = groupTweens[i];

                // Remove null tweens and empty groups
                if (t is null or TweenGroup { tweens: null or { Count: 0 }}) {
                    groupTweens.RemoveAt(i--);
                    continue;
                }

                if (t is TweenGroup g) {
                    switch (g.mode) {
                        case TweenGroup.Mode.Parallel:
                            // If new tween is also a parallel group,
                            // then try to collect maximum parallel groups into one root parallel group.
                            groupTweens.AddRange(g.tweens);
                            groupTweens.RemoveAt(i--);
                            break;

                        case TweenGroup.Mode.Sequential:
                            // If new tween is a sequential group with a single element, then add only the element,
                            // otherwise add the group into parallel tweens.
                            groupTweens.Add(g.tweens.Count == 1 ? g.tweens[0] : g);
                            break;
                    }
                }
            }
        }

        private static async UniTask PlayDelayed(
            ITween tween,
            float delay,
            float duration,
            float startProgress,
            float speed,
            CancellationToken cancellationToken = default
        ) {
            if (tween == null) return;

            bool canceled = await UniTask
                .Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();

            if (canceled) return;

            await tween.Play(duration, startProgress, speed, cancellationToken);
        }
    }

}
