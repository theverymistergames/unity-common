using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.ActionLib.Animation {
    
    [Serializable]
    public sealed class BlendShapeAction : IActorAction {
        
        public SkinnedMeshRenderer skinnedMeshRenderer;
        [Min(0)] public int index;
        [Min(0f)] public float duration;
        [Range(0f, 100f)] public float endWeight;
        public AnimationCurve curve = EasingType.Linear.ToAnimationCurve();

        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            float startWeight = skinnedMeshRenderer.GetBlendShapeWeight(index);
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            float t = 0f;

            while (!cancellationToken.IsCancellationRequested) {
                t = Mathf.Clamp01(t + UnityEngine.Time.deltaTime * speed);
                
                skinnedMeshRenderer.SetBlendShapeWeight(index, Mathf.Lerp(startWeight, endWeight, curve.Evaluate(t)));
                await UniTask.Yield();
            }
        }
    }
    
}