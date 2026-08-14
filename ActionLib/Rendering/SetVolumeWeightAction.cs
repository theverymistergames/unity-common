using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.Rendering;

namespace MisterGames.ActionLib.Rendering {

    [Serializable]
    public sealed class SetVolumeWeightAction : IActorAction {

        public Volume volume;
        [Range(0f, 1f)] public float weight;
        [Min(0f)] public float duration;
        public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public bool wait;
        public bool useUnscaledTime;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (wait) return SetWeight(cancellationToken);
            
            SetWeight(cancellationToken).Forget();
            return default;
        }

        private async UniTask SetWeight(CancellationToken cancellationToken) {
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            float startWeight = volume.weight;
            
            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                float dt = useUnscaledTime ? TimeSources.unscaledDeltaTime : TimeSources.deltaTime; 
                t = Mathf.Clamp01(t + dt * speed);

                volume.weight = Mathf.Lerp(startWeight, weight, curve.Evaluate(t));
                
                await UniTask.Yield();
            }
        }
    }
    
}