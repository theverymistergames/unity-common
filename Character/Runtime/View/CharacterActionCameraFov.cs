using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Data;
using MisterGames.Tick.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Character.View {

    [Serializable]
    public sealed class CharacterActionCameraFov : ICharacterAction {

        public PlayerLoopStage playerLoopStage = PlayerLoopStage.Update;
        public bool keepChanges;

        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandom;
        public float weight = 1f;

        public FloatParameter fovOffset = FloatParameter.Default();

        public async UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var cameraContainer = characterAccess
                .GetPipeline<CharacterViewPipeline>()
                .CameraContainer;

            var timeSource = TimeSources.Get(playerLoopStage);

            float progress = 0f;
            float resultDuration = duration + Random.Range(-durationRandom, durationRandom);

            float m = fovOffset.CreateMultiplier();
            var key = cameraContainer.CreateState(this, weight);

            while (!cancellationToken.IsCancellationRequested) {
                float progressDelta = resultDuration <= 0f ? 1f : timeSource.DeltaTime / resultDuration;
                progress = Mathf.Clamp01(progress + progressDelta);

                float fov = m * fovOffset.Evaluate(progress);
                cameraContainer.SetFovOffset(key, fov);

                if (progress >= 1f) break;

                await UniTask.Yield();
            }

            cameraContainer.RemoveState(key, keepChanges);
        }
    }

}
