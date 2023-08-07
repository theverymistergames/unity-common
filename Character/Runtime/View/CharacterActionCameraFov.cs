using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;
using MisterGames.Tick.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Character.View {

    [Serializable]
    public sealed class CharacterActionCameraFov : IAsyncAction, IDependency {

        public PlayerLoopStage playerLoopStage = PlayerLoopStage.Update;
        public bool keepChanges;

        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandom;
        public float weight = 1f;

        public FloatParameter fovOffset = FloatParameter.Default();

        private CameraContainer _cameraContainer;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _cameraContainer = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<CharacterViewPipeline>()
                .CameraContainer;
        }

        public async UniTask Apply(object source, CancellationToken cancellationToken = default) {
            var timeSource = TimeSources.Get(playerLoopStage);

            float progress = 0f;
            float resultDuration = duration + Random.Range(-durationRandom, durationRandom);

            float m = fovOffset.CreateMultiplier();
            var key = _cameraContainer.CreateState(this, weight);

            while (!cancellationToken.IsCancellationRequested) {
                float progressDelta = resultDuration <= 0f ? 1f : timeSource.DeltaTime / resultDuration;
                progress = Mathf.Clamp01(progress + progressDelta);

                float fov = m * fovOffset.Evaluate(progress);
                _cameraContainer.SetFovOffset(key, fov);

                if (progress >= 1f) break;

                await UniTask.Yield();
            }

            _cameraContainer.RemoveState(key, keepChanges);
        }
    }

}
