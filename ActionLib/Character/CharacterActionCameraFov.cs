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

        public PlayerLoopStage playerLoopStage = PlayerLoopStage.Update;

        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandom;
        public float weight = 1f;

        public FloatParameter fovOffset = FloatParameter.Default();

        public async UniTask Apply(IActor actor, CancellationToken cancellationToken = default) {
            var cameraContainer = actor.GetComponent<CharacterViewPipeline>().CameraContainer;

            var timeSource = TimeSources.Get(playerLoopStage);

            float progress = 0f;
            float resultDuration = duration + Random.Range(-durationRandom, durationRandom);

            float m = fovOffset.CreateMultiplier();
            int id = cameraContainer.CreateState(weight);

            while (!cancellationToken.IsCancellationRequested) {
                float progressDelta = resultDuration <= 0f ? 1f : timeSource.DeltaTime / resultDuration;
                progress = Mathf.Clamp01(progress + progressDelta);

                float fov = m * fovOffset.Evaluate(progress);
                cameraContainer.SetFovOffset(id, fov);

                if (progress >= 1f) break;

                await UniTask.Yield();
            }

            cameraContainer.RemoveState(id);
        }
    }

}
