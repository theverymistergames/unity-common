using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.ActionLib.Tweens {

    [Serializable]
    public sealed class PlayTweenProgressAction : IActorAction {

        public float startProgress;
        public float endProgress = 1f;
        [Min(0f)] public float duration;
        public AnimationCurve curve = EasingType.Linear.ToAnimationCurve();
        [SerializeReference] [SubclassSelector] public ITweenProgressAction action;
        public bool useUnscaledTime;

        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            float t = 0f;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;

            while (!cancellationToken.IsCancellationRequested) {
                float dt = useUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
                t = Mathf.Clamp01(t + dt * speed);
                
                action?.OnProgressUpdate(Mathf.Lerp(startProgress, endProgress, curve.Evaluate(t)));
                
                if (t >= 1f) break;
                
                await UniTask.Yield();
            }
        }
    }
    
}