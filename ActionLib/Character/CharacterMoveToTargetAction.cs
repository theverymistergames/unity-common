using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Collisions;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterMoveToTargetAction : IActorAction {
        
        [Header("Target")]
        public TargetType targetType;
        [VisibleIf(nameof(targetType), 0)] public Transform target;
        [VisibleIf(nameof(targetType), 0)] public bool saveHeadPosition;
        public Vector3 offset;
        public Vector3 rotationOffset;
        public OffsetMode offsetMode;
        
        [Header("Motion")]
        public bool disableCollisionsWhileMoving = true;
        [Min(0f)] public float speed = 1f;
        [Min(0f)] public float reduceSpeedBelowDistance = 0.1f;
        [Range(0f, 1f)] public float speedCoeffMin = 0.01f;
        public float curvature = 1f;
        public AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        [Header("Ground")]
        public bool detectGround;
        [VisibleIf(nameof(detectGround), 0)] public bool allowTranslationY;
        [VisibleIf(nameof(detectGround))] public LayerMask layer;
        [VisibleIf(nameof(detectGround))] public float rayElevation;
        [VisibleIf(nameof(detectGround))] public float maxDistance;
        [VisibleIf(nameof(detectGround))] public float groundOffset;

        public enum TargetType {
            Transform,
            Head,
        }
        
        public enum OffsetMode {
            Local,
            World,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return targetType switch {
                TargetType.Transform => MoveToTransform(context, cancellationToken),
                TargetType.Head => MoveToHead(context, cancellationToken),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public async UniTask MoveToTransform(IActor context, CancellationToken cancellationToken = default) {
            var view = context.GetComponent<CharacterViewPipeline>();
            var collisions = context.GetComponent<CharacterCollisionPipeline>();

            target.GetPositionAndRotation(out var targetStartPosition, out var targetStartRotation);
            
            var finalRotation =
                (offsetMode == OffsetMode.Local ? targetStartRotation : Quaternion.identity) * 
                Quaternion.Euler(rotationOffset);

            var startPoint = view.BodyPosition;
            var finalPoint = targetStartPosition + finalRotation * offset;

            var up = view.BodyUp;
            
            if (detectGround) {
                bool hasHit = Physics.Raycast(
                    finalPoint + up * rayElevation,
                    -up,
                    out var hit,
                    maxDistance,
                    layer,
                    QueryTriggerInteraction.Ignore
                );

                if (hasHit) {
                    finalPoint = hit.point + up * groundOffset;
                }
            } 
            else if (!allowTranslationY) {
                finalPoint = startPoint + Vector3.ProjectOnPlane(finalPoint - startPoint, up);
            }
            
            var curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, finalPoint, finalRotation, curvature);
            
            var startPointOffset = startPoint - targetStartPosition;
            var curvePointOffset = curvePoint - targetStartPosition;
            var finalPointOffset = finalPoint - targetStartPosition;
            
            float pathLength = BezierExtensions.GetBezier3PointsLength(startPoint, curvePoint, finalPoint);
            float speed = pathLength > 0f && this.speed > 0f ? this.speed / pathLength : float.MaxValue;
            float t = 0f;

            if (disableCollisionsWhileMoving) collisions.enabled = false;
            
            while (!cancellationToken.IsCancellationRequested) {
                target.GetPositionAndRotation(out var targetPosition, out var targetRotation);
                
                var targetRotationOffset = targetRotation * Quaternion.Inverse(targetStartRotation);

                startPoint = targetPosition + targetRotationOffset * startPointOffset;
                curvePoint = targetPosition + targetRotationOffset * curvePointOffset;
                finalPoint = targetPosition + targetRotationOffset * finalPointOffset;

                var bodyPos = view.BodyPosition;
                
                float distance = (finalPoint - bodyPos).magnitude;
                float dt = UnityEngine.Time.deltaTime;
                float k = reduceSpeedBelowDistance > 0f ? Mathf.Clamp01(speedCoeffMin + distance / reduceSpeedBelowDistance) : 1f;

                t = Mathf.Clamp01(t + speed * k * dt);
                
                var position = BezierExtensions.EvaluateBezier3Points(
                    startPoint,
                    curvePoint,
                    finalPoint,
                    progressCurve.Evaluate(t)
                );

                var headPos = view.HeadPosition;
                
                view.BodyPosition = position;
                if (saveHeadPosition) view.HeadPosition = headPos;
                
#if UNITY_EDITOR
                DebugExt.DrawSphere(view.BodyPosition, 0.005f, Color.yellow, duration: 5f);
#endif
                
                if (t >= 1f) break;
                
                await UniTask.Yield();
            }
            
#if UNITY_EDITOR
            DebugExt.DrawSphere(view.BodyPosition, 0.01f, Color.green, duration: 5f);
#endif
            
            if (disableCollisionsWhileMoving) collisions.enabled = true;
        }

        public async UniTask MoveToHead(IActor context, CancellationToken cancellationToken = default) {
            var view = context.GetComponent<CharacterViewPipeline>();
            var collisions = context.GetComponent<CharacterCollisionPipeline>();

            var targetStartPosition = view.HeadPosition;
            
            var targetRotation =
                (offsetMode == OffsetMode.Local ? view.HeadRotation : Quaternion.identity) * 
                Quaternion.Euler(rotationOffset);

            var startPoint = view.BodyPosition;
            var finalPoint = view.HeadPosition + targetRotation * offset;
            
            var up = view.BodyUp;
            
            if (detectGround) {
                bool hasHit = Physics.Raycast(
                    finalPoint + up * rayElevation,
                    -up,
                    out var hit,
                    maxDistance,
                    layer,
                    QueryTriggerInteraction.Ignore
                );

                if (hasHit) {
                    finalPoint = hit.point + up * groundOffset;
                }
            } 
            else if (!allowTranslationY) {
                finalPoint = startPoint + Vector3.ProjectOnPlane(finalPoint - startPoint, up);
            }
            
            var curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, finalPoint, targetRotation, curvature);
            
            var startPointOffset = startPoint - targetStartPosition;
            var curvePointOffset = curvePoint - targetStartPosition;
            var finalPointOffset = finalPoint - targetStartPosition;
            
            float pathLength = BezierExtensions.GetBezier3PointsLength(startPoint, curvePoint, finalPoint);
            float speed = pathLength > 0f && this.speed > 0f ? this.speed / pathLength : float.MaxValue;
            float t = 0f;
            
            if (disableCollisionsWhileMoving) collisions.enabled = false;

            while (!cancellationToken.IsCancellationRequested) {
                var targetPosition = view.HeadPosition;

                startPoint = targetPosition + startPointOffset;
                curvePoint = targetPosition + curvePointOffset;
                finalPoint = targetPosition + finalPointOffset;
                
                float distance = (finalPoint - view.BodyPosition).magnitude;
                float dt = UnityEngine.Time.deltaTime;
                float k = reduceSpeedBelowDistance > 0f ? Mathf.Clamp01(speedCoeffMin + distance / reduceSpeedBelowDistance) : 1f;

                t = Mathf.Clamp01(t + speed * k * dt);
                
                var position = BezierExtensions.EvaluateBezier3Points(
                    startPoint,
                    curvePoint,
                    finalPoint,
                    progressCurve.Evaluate(t)
                );

                var headPos = view.HeadPosition;
                
                view.BodyPosition = position;
                view.HeadPosition = headPos;
                
#if UNITY_EDITOR
                DebugExt.DrawSphere(view.BodyPosition, 0.005f, Color.yellow, duration: 5f);
#endif
                
                if (t >= 1f) break;
                
                await UniTask.Yield();
            }
            
#if UNITY_EDITOR
            DebugExt.DrawSphere(view.BodyPosition, 0.01f, Color.green, duration: 5f);
#endif
            
            if (disableCollisionsWhileMoving) collisions.enabled = true;
        }
    }
    
}