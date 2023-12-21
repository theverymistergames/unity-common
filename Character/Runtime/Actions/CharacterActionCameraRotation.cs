using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Character.View;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionCameraRotation : ICharacterAction {

        public PlayerLoopStage playerLoopStage = PlayerLoopStage.Update;
        public bool keepChanges;

        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandom;

        public float weight = 1f;
        public float baseMultiplier = 1f;
        public float baseMultiplierRandom = 0f;

        public Vector3Parameter eulers = Vector3Parameter.Default();

        public async UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var cameraContainer = characterAccess
                .GetPipeline<CharacterViewPipeline>()
                .CameraContainer;

            var timeSource = TimeSources.Get(playerLoopStage);

            float progress = 0f;
            float resultDuration = duration + Random.Range(-durationRandom, durationRandom);

            var m = (baseMultiplier + Random.Range(-baseMultiplierRandom, baseMultiplierRandom)) * eulers.CreateMultiplier();
            var key = cameraContainer.CreateState(this, weight);

            while (!cancellationToken.IsCancellationRequested) {
                float progressDelta = resultDuration <= 0f ? 1f : timeSource.DeltaTime / resultDuration;
                progress = Mathf.Clamp01(progress + progressDelta);

                var rotation = eulers.Evaluate(progress).Multiply(m);
                cameraContainer.SetRotationOffset(key, Quaternion.Euler(rotation));

                if (progress >= 1f) break;

                await UniTask.Yield();
            }

            cameraContainer.RemoveState(key, keepChanges);
        }
    }

}
