using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterRotateHeadAction : IActorAction {

        public Vector3 orientation;
        [Min(0f)] public float angularSpeed = 30f;
        public AnimationCurve progressCurve = EasingType.EaseOutSine.ToAnimationCurve();
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var view = context.GetComponent<CharacterViewPipeline>();
            
            var startRotation = view.HeadRotation;
            var targetRotation = Quaternion.Euler(orientation);

            float angle = Quaternion.Angle(startRotation, targetRotation);
            float speed = angularSpeed > 0f && angle > 0f ? angularSpeed / angle : float.MaxValue;
            float t = 0f;
            
            view.SetViewOrientation(startRotation, moveView: false);
            
            while (!cancellationToken.IsCancellationRequested) {
                t += UnityEngine.Time.deltaTime * speed;
                
                var rot = Quaternion.Slerp(startRotation, targetRotation, progressCurve.Evaluate(t));
                
                view.SetViewOrientation(rot, moveView: true);
                
                if (t >= 1f) break;
                
#if UNITY_EDITOR
                DebugExt.DrawRay(view.HeadPosition, view.HeadRotation * Vector3.forward, Color.yellow, duration: 3f);
                DebugExt.DrawRay(view.HeadPosition, targetRotation * Vector3.forward, Color.green, duration: 3f);
#endif
                
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            }
        }
    }
    
}