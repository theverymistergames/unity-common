using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.ActionLib.Sounds;
using MisterGames.Actors;
using MisterGames.Character.Core;
using MisterGames.Common.Async;
using MisterGames.Common.Audio;
using MisterGames.Common.Tick;
using UnityEngine;
using Random = UnityEngine.Random;
using MisterGames.Common;

namespace MisterGames.Character.Behaviours {

    public sealed class PlaySoundAroundCharacter : MonoBehaviour {

        [SerializeField] [Min(0f)] private float _spawnCooldown = 1f;
        [SerializeField] private SoundSource[] _sources;

        [Serializable]
        private struct SoundSource {
            [Header("Series")]
            [Min(1)] public int countMin;
            [Min(1)] public int countMax;
            [Min(0f)] public float delayMin;
            [Min(0f)] public float delayMax;

            [Header("Cooldown")]
            [Min(0f)] public float cooldownMin;
            [Min(0f)] public float cooldownMax;

            [Header("Position")]
            [Min(0f)] public float radiusMin;
            [Min(0f)] public float radiusMax;
            public float heightMin;
            public float heightMax;

            [Header("Sound")]
            public PlaySoundAction playSoundAction;

            [Header("Debug")]
            public bool showGizmo;
        }

        private CancellationTokenSource _enableCts;
        private float _nextSpawnTime;

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);

            for (int i = 0; i < _sources.Length; i++) {
                PlaySourceLoop(_sources[i], _enableCts.Token).Forget();
            }
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
        }

        private async UniTask PlaySourceLoop(SoundSource source, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                await AsyncExt.Delay(Random.Range(source.cooldownMin, source.cooldownMax), cancellationToken);

                while (TimeSources.scaledTime < _nextSpawnTime && !cancellationToken.IsCancellationRequested) {
                    await UniTask.Yield();
                }

                if (cancellationToken.IsCancellationRequested) return;

                _nextSpawnTime = TimeSources.scaledTime + _spawnCooldown;

                if (CharacterSpawner.Instance == null || CharacterSpawner.Instance.GetCharacter() is not { } character) continue;

                var point = character.Transform.position + GetRandomOffset(ref source);
                int count = Random.Range(source.countMin, source.countMax + 1);

                for (int i = 0; i < count && !cancellationToken.IsCancellationRequested; i++) {
                    PlaySound(ref source, point, character, cancellationToken);

                    await AsyncExt.Delay(Random.Range(source.delayMin, source.delayMax), cancellationToken);
                }
            }
        }

        private static void PlaySound(ref SoundSource source, Vector3 point, IActor character, CancellationToken cancellationToken) {
            if (source.playSoundAction.audioClipVariants is not { Length: > 0 } || AudioPool.Main is not { } pool) return;

            var clip = pool.ShuffleClips(source.playSoundAction.audioClipVariants);

            source.playSoundAction.PlaySoundNoAttach(clip, point, source.playSoundAction.volume, character, cancellationToken);
        }

        private static Vector3 GetRandomOffset(ref SoundSource source) {
            float closestHeight = Mathf.Max(0f, source.heightMin, -source.heightMax);
            float radius = Random.Range(Mathf.Max(source.radiusMin, closestHeight), source.radiusMax);
            float height = Random.Range(Mathf.Max(source.heightMin, -radius), Mathf.Min(source.heightMax, radius));
            float horizontalRadius = Mathf.Sqrt(Mathf.Max(0f, radius * radius - height * height));
            float angle = Random.Range(0f, 2f * Mathf.PI);

            return new Vector3(Mathf.Cos(angle) * horizontalRadius, height, Mathf.Sin(angle) * horizontalRadius);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            if (_sources == null) return;

            var center = Application.isPlaying &&
                         CharacterSpawner.Instance != null &&
                         CharacterSpawner.Instance.GetCharacter() is { } character
                ? character.Transform.position
                : transform.position;

            for (int i = 0; i < _sources.Length; i++) {
                if (_sources[i].showGizmo) DrawSourceGizmo(center, _sources[i]);
            }
        }

        private static void DrawSourceGizmo(Vector3 center, SoundSource source) {
            DrawSlicedSphere(center, source.radiusMin, source.heightMin, source.heightMax, Color.yellow);
            DrawSlicedSphere(center, source.radiusMax, source.heightMin, source.heightMax, Color.green);
        }

        private static void DrawSlicedSphere(Vector3 center, float radius, float heightMin, float heightMax, Color color) {
            float heightBottom = Mathf.Max(heightMin, -radius);
            float heightTop = Mathf.Min(heightMax, radius);

            if (radius <= 0f || heightBottom > heightTop) return;

            float elevationMin = Mathf.Asin(heightBottom / radius) * Mathf.Rad2Deg;
            float elevationMax = Mathf.Asin(heightTop / radius) * Mathf.Rad2Deg;

            DebugExt.DrawCircle(center + Vector3.up * heightBottom, Quaternion.identity, Mathf.Sqrt(radius * radius - heightBottom * heightBottom), color, gizmo: true);
            DebugExt.DrawCircle(center + Vector3.up * heightTop, Quaternion.identity, Mathf.Sqrt(radius * radius - heightTop * heightTop), color, gizmo: true);

            for (int i = 0; i < 4; i++) {
                var rotation = Quaternion.AngleAxis(i * 90f, Vector3.up);
                var dir = rotation * Vector3.right;
                var normal = rotation * Vector3.forward;
                var orientation = Quaternion.LookRotation(Quaternion.AngleAxis(elevationMin, normal) * dir, normal);

                DebugExt.DrawCircle(center, orientation, radius, color, angle: elevationMax - elevationMin, gizmo: true);
            }
        }
#endif
    }

}
