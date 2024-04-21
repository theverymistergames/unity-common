using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterActionCameraRotation : IActorAction {

        public PlayerLoopStage playerLoopStage = PlayerLoopStage.Update;

        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandom;

        public float weight = 1f;
        public float baseMultiplier = 1f;
        public float baseMultiplierRandom = 0f;

        public Vector3Parameter eulers = Vector3Parameter.Default();

        public async UniTask Apply(IActor actor, CancellationToken cancellationToken = default) {
            var cameraContainer = actor.GetComponent<CharacterViewPipeline>().CameraContainer;

            var timeSource = TimeSources.Get(playerLoopStage);

            float progress = 0f;
            float resultDuration = duration + Random.Range(-durationRandom, durationRandom);

            var m = (baseMultiplier + Random.Range(-baseMultiplierRandom, baseMultiplierRandom)) * eulers.CreateMultiplier();
            int id = cameraContainer.CreateState(weight);

            while (!cancellationToken.IsCancellationRequested) {
                float progressDelta = resultDuration <= 0f ? 1f : timeSource.DeltaTime / resultDuration;
                progress = Mathf.Clamp01(progress + progressDelta);

                var rotation = eulers.Evaluate(progress).Multiply(m);
                cameraContainer.SetRotationOffset(id, Quaternion.Euler(rotation));

                if (progress >= 1f) break;

                await UniTask.Yield();
            }

            cameraContainer.RemoveState(id);
        }
    }

}
