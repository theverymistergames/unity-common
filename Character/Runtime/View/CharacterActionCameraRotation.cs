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
    public sealed class CharacterActionCameraRotation : IAsyncAction, IDependency {

        public PlayerLoopStage playerLoopStage = PlayerLoopStage.Update;
        public bool keepChanges;

        [Min(0f)] public float duration;
        [Min(0f)] public float durationRandom;
        public float weight = 1f;

        public Optional<Parameter> x = Default();
        public Optional<Parameter> y = Default();
        public Optional<Parameter> z = Default();

        private CameraContainer _cameraContainer;

        [Serializable]
        public struct Parameter {
            public float multiplier;
            public float multiplierRandom;
            public AnimationCurve curve;
        }

        private static Optional<Parameter> Default() => new Optional<Parameter>(
            new Parameter {
                multiplier = 1f,
                multiplierRandom = 0f,
                curve = AnimationCurve.Linear(0f, 0f, 1f, 1f)
            },
            hasValue: false
        );

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

            float mX = x.Value.multiplier + Random.Range(-x.Value.multiplierRandom, x.Value.multiplierRandom);
            float mY = y.Value.multiplier + Random.Range(-y.Value.multiplierRandom, y.Value.multiplierRandom);
            float mZ = z.Value.multiplier + Random.Range(-z.Value.multiplierRandom, z.Value.multiplierRandom);

            var key = _cameraContainer.CreateState(this, weight);

            while (!cancellationToken.IsCancellationRequested) {
                float progressDelta = resultDuration <= 0f ? 1f : timeSource.DeltaTime / resultDuration;
                progress = Mathf.Clamp01(progress + progressDelta);

                var eulers = Vector3.zero;
                if (x.HasValue) eulers.x += mX * x.Value.curve.Evaluate(progress);
                if (y.HasValue) eulers.y += mY * y.Value.curve.Evaluate(progress);
                if (z.HasValue) eulers.z += mZ * z.Value.curve.Evaluate(progress);

                _cameraContainer.SetRotationOffset(key, Quaternion.Euler(eulers));

                if (progress >= 1f) break;

                await UniTask.Yield();
            }

            _cameraContainer.RemoveState(key, keepChanges);
        }
    }

}
