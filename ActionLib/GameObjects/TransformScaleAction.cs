using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class TransformScaleAction : IActorAction {
        
        public Transform transform;
        [Min(0f)] public float duration;
        public Vector3 endScale;
        public AnimationCurve curve = EasingType.Linear.ToAnimationCurve();
        public bool useUnscaledTime;

        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var startScale = transform.localScale;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            float t = 0f;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                float dt = useUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
                t = Mathf.Clamp01(t + dt * speed);
                
                transform.localScale = Vector3.Lerp(startScale, endScale, curve.Evaluate(t));
                await UniTask.Yield();
            }
        }
    }
    
}