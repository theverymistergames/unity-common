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
    public sealed class CharacterLookAtTargetAction : IActorAction {

        [Header("Target")]
        public Transform target;
        public LookAtMode mode;
        [VisibleIf(nameof(mode), 1)] public Vector3 orientation;

        [Header("Motion")]
        [Min(0f)] public float angularSpeed = 30f;
        [Range(0f, 180f)] public float maxAngleDiff;
        public AnimationCurve progressCurve = EasingType.EaseOutSine.ToAnimationCurve();
        
        [Header("Attach")]
        public bool keepLookingAtAfterFinish;
        [Min(0f)] public float attachSmoothing;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var view = context.GetComponent<CharacterViewPipeline>();
            
            var startRotation = view.HeadRotation;
            var startTargetRotation = mode switch {
                LookAtMode.Free => Quaternion.LookRotation(target.position - view.HeadPosition, view.BodyUp),
                LookAtMode.Oriented => target.rotation * Quaternion.Euler(orientation),
                _ => throw new ArgumentOutOfRangeException()
            };

            float angle = Quaternion.Angle(startRotation, startTargetRotation);
            float speed = angularSpeed > 0f && angle > 0f ? angularSpeed / angle : float.MaxValue;
            float t = 0f;
            
            while (speed < float.MaxValue && !cancellationToken.IsCancellationRequested) {
                var targetRotation = mode switch {
                    LookAtMode.Free => Quaternion.LookRotation(target.position - view.HeadPosition, view.BodyUp),
                    LookAtMode.Oriented => target.rotation * Quaternion.Euler(orientation),
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                t += UnityEngine.Time.deltaTime * speed;
                
                var rotationOffset = targetRotation * Quaternion.Inverse(startTargetRotation);
                var rot = Quaternion.Slerp(startRotation * rotationOffset, targetRotation, progressCurve.Evaluate(t));
                
                view.SetViewOrientation(rot);
                
                if (t >= 1f) break;
                
#if UNITY_EDITOR
                DebugExt.DrawRay(view.HeadPosition, view.HeadRotation * Vector3.forward, Color.yellow, duration: 3f);
                DebugExt.DrawRay(view.HeadPosition, targetRotation * Vector3.forward, Color.green, duration: 3f);
#endif
                
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            }
            
            if (cancellationToken.IsCancellationRequested) return;
            
            if (keepLookingAtAfterFinish) view.LookAt(target, Vector3.zero, mode, orientation, attachSmoothing);
        }
    }
    
}