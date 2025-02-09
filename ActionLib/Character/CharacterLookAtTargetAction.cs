using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.Maths;
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
            
            var rotation = view.HeadRotation;
            var targetRotation = mode switch {
                LookAtMode.Free => Quaternion.LookRotation(target.position - view.HeadPosition, view.BodyUp),
                LookAtMode.Oriented => target.rotation * Quaternion.Euler(orientation),
                _ => throw new ArgumentOutOfRangeException()
            };

            float speed = angularSpeed > 0f ? angularSpeed : float.MaxValue;
            float angle = Quaternion.Angle(rotation, targetRotation);
            float invAngle = angle > 0f ? 1f / angle : 0f;
            float tMin = 0f;
            
            while (speed < float.MaxValue && !cancellationToken.IsCancellationRequested) {
                float dt = UnityEngine.Time.deltaTime;
                
                targetRotation = mode switch {
                    LookAtMode.Free => Quaternion.LookRotation(target.position - view.HeadPosition, view.BodyUp),
                    LookAtMode.Oriented => target.rotation * Quaternion.Euler(orientation),
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                // Find speed via progress curve derivative
                float t0 = Mathf.Max(tMin, Mathf.Clamp01(1f - Quaternion.Angle(rotation, targetRotation) * invAngle));
                float t1 = Mathf.Min(1f, t0 + speed * dt);
                
                tMin = t0;
                
                float dx = t1 <= t0 ? 1f : (progressCurve.Evaluate(t1) - progressCurve.Evaluate(t0)) / (t1 - t0);

                rotation = Quaternion.RotateTowards(rotation, targetRotation, speed * dt * dx);
                view.SetViewCenter(rotation);
                
                if (Quaternion.Angle(rotation, targetRotation).IsNearlyZero(tolerance: maxAngleDiff)) break;
                    
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