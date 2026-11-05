using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Tick;
using MisterGames.Logic.Rendering;
using UnityEngine;

namespace MisterGames.ActionLib.Rendering {

    [Serializable]
    public sealed class SetLightIntensityNoiseMulAction : IActorAction {

        public LightIntensityNoise lightIntensityNoise;
        [Min(0f)] public float intensityMul = 1f;
        [Min(0f)] public float duration;
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public bool wait;
        public bool useUnscaledTime;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (wait) return SetIntensityMul(cancellationToken);

            SetIntensityMul(cancellationToken).Forget();
            return default;
        }

        private async UniTask SetIntensityMul(CancellationToken cancellationToken) {
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            float startMul = lightIntensityNoise.IntensityMul;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                float dt = useUnscaledTime ? TimeSources.unscaledDeltaTime : TimeSources.deltaTime;
                t = Mathf.Clamp01(t + dt * speed);

                lightIntensityNoise.IntensityMul = Mathf.Lerp(startMul, intensityMul, curve.Evaluate(t));

                await UniTask.Yield();
            }
        }
    }

}
