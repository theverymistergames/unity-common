using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterActionCameraFov : IActorAction {

        public bool keepChanges;
        public float weight = 1f;
        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandom;

        public FloatParameter fovOffset = FloatParameter.Default;

        public async UniTask Apply(IActor actor, CancellationToken cancellationToken = default) {
            var cameraContainer = actor.GetComponent<CameraContainer>();

            var timeSource = PlayerLoopStage.LateUpdate.Get();

            float progress = 0f;
            float resultDuration = duration + Random.Range(-durationRandom, durationRandom);

            float m = fovOffset.CreateMultiplier();
            int id = cameraContainer.CreateState();

            while (!cancellationToken.IsCancellationRequested) {
                float progressDelta = resultDuration <= 0f ? 1f : timeSource.DeltaTime / resultDuration;
                progress = Mathf.Clamp01(progress + progressDelta);

                float fov = m * fovOffset.Evaluate(progress);
                cameraContainer.SetFovOffset(id, weight, fov);

                if (progress >= 1f) break;

                await UniTask.Yield();
            }

            cameraContainer.RemoveState(id, keepChanges);
        }
    }

}
