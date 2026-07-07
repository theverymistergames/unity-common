using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;
using UnityEngine.Rendering;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class ChangeVolumeWeightAction : IActorAction {
        
        public Volume volume;
        [Range(0f, 1f)] public float weight;
        [Min(0f)] public float duration;
        public bool useUnscaledTime;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            float start = volume.weight;
            float dur = duration * Mathf.Abs(start - weight);
            float speed = dur > 0f ? 1f / dur : float.MaxValue;
            float t = 0f;

            while (!cancellationToken.IsCancellationRequested && t < 1f) {
                float dt = useUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
                t = Mathf.Clamp01(t + speed * dt);
                volume.weight = Mathf.Lerp(start, weight, t);
                
                await UniTask.Yield();
            }
        }
    }
    
}