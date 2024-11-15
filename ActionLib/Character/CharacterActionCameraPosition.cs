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
    public sealed class CharacterActionCameraPosition : IActorAction {

        public bool keepChanges;
        public float weight = 1f;
        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandom;
        public float baseMultiplier = 1f;
        public float baseMultiplierRandom = 0f;

        public Vector3Parameter offset = Vector3Parameter.Default();

        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var cameraContainer = context.GetComponent<CameraContainer>();

            var timeSource = PlayerLoopStage.LateUpdate.Get();

            float progress = 0f;
            float resultDuration = duration + Random.Range(-durationRandom, durationRandom);

            var m = (baseMultiplier + Random.Range(-baseMultiplierRandom, baseMultiplierRandom)) * offset.CreateMultiplier();
            int id = cameraContainer.CreateState();

            while (!cancellationToken.IsCancellationRequested) {
                float progressDelta = resultDuration <= 0f ? 1f : timeSource.DeltaTime / resultDuration;
                progress = Mathf.Clamp01(progress + progressDelta);

                var position = offset.Evaluate(progress).Multiply(m);
                cameraContainer.SetPositionOffset(id, weight, position);

                if (progress >= 1f) break;

                await UniTask.Yield();
            }

            if (cancellationToken.IsCancellationRequested) return;
            
            cameraContainer.RemoveState(id, keepChanges);
        }
    }

}
