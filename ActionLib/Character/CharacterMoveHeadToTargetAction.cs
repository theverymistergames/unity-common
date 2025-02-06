using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterMoveHeadToTargetAction : IActorAction {

        [Header("Target")]
        public TargetType targetType;
        [VisibleIf(nameof(targetType), 0)] public Vector3 localPosition;
        [VisibleIf(nameof(targetType), 0)] public bool useTargetAsCurvePointOrigin;
        public Transform target;
        [VisibleIf(nameof(targetType), 1)] public OffsetMode offsetMode;
        [VisibleIf(nameof(targetType), 1)] public Vector3 offset;
        public Vector3 rotationOffset;
        [VisibleIf(nameof(targetType), 1)] public bool attach;
        [VisibleIf(nameof(targetType), 1)] [Min(0f)] public float attachSmoothing;
        [VisibleIf(nameof(targetType), 1)] public AttachMode attachMode;
        
        [Header("Motion")]
        [Min(0f)] public float speed = 1f;
        [Min(0f)] public float reduceSpeedBelowDistance = 0.5f;
        [Min(0f)] public float speedMin = 0.01f;
        public float curvature = 1f;
        public AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        public enum TargetType {
            LocalPosition,
            Transform,
        }

        public enum OffsetMode {
            Local,
            World,
            UseViewDirectionAsForward,
        }

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            return targetType switch {
                TargetType.LocalPosition => MoveHeadToLocalPosition(context, cancellationToken),
                TargetType.Transform => MoveHeadToTransform(context, cancellationToken),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private async UniTask MoveHeadToLocalPosition(IActor context, CancellationToken cancellationToken) {
            var view = context.GetComponent<CharacterViewPipeline>();

            Vector3 startPoint;
            Vector3 targetPoint;
            Vector3 curvePoint;

            if (useTargetAsCurvePointOrigin) {
                startPoint = localPosition;
                targetPoint = view.HeadLocalPosition;
                var rot = target.rotation * Quaternion.Euler(rotationOffset) * Quaternion.Inverse(view.BodyRotation);
                curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, rot, curvature);
            }
            else {
                startPoint = view.HeadLocalPosition;
                targetPoint = localPosition;
                var rot = 
                    Quaternion.LookRotation(targetPoint.sqrMagnitude > 0f ? -targetPoint : startPoint, Vector3.up) * 
                    Quaternion.Euler(rotationOffset);
                        
                curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, rot, curvature);
            }

            float pathLength = BezierExtensions.GetBezier3PointsLength(startPoint, curvePoint, targetPoint);
            float speed = pathLength > 0f ? this.speed / pathLength : float.MaxValue;
            float t = 0f;
            
            while (!cancellationToken.IsCancellationRequested) {
                var diff = localPosition - view.HeadLocalPosition;

                float dt = UnityEngine.Time.deltaTime;
                float k = reduceSpeedBelowDistance > 0f ? Mathf.Clamp01(diff.magnitude / reduceSpeedBelowDistance) : 1f;
                t = Mathf.Clamp01(t + dt * Mathf.Max(speed * k, speedMin));

                var position = BezierExtensions.EvaluateBezier3Points(
                    startPoint,
                    curvePoint,
                    targetPoint,
                    useTargetAsCurvePointOrigin ? 1f - progressCurve.Evaluate(t) : progressCurve.Evaluate(t)
                );

                view.HeadLocalPosition = position;
                
#if UNITY_EDITOR
                DebugExt.DrawSphere(view.HeadPosition, 0.008f, Color.yellow, duration: 5f);
#endif
                
                if (t >= 1f) break;
                
                await UniTask.Yield();
            }
            
#if UNITY_EDITOR
            DebugExt.DrawSphere(view.HeadPosition, 0.01f, Color.green, duration: 5f);
#endif
        }

        private async UniTask MoveHeadToTransform(IActor context, CancellationToken cancellationToken) {
            var view = context.GetComponent<CharacterViewPipeline>();
            
            target.GetPositionAndRotation(out var targetStartPosition, out var targetStartRotation);
            
            var headStartRotation = view.HeadRotation;
            var startPoint = view.HeadPosition;

            var offsetOrient = offsetMode switch {
                OffsetMode.Local => targetStartRotation * Quaternion.Euler(rotationOffset),
                OffsetMode.World => Quaternion.Euler(rotationOffset),
                OffsetMode.UseViewDirectionAsForward => Quaternion.LookRotation(targetStartPosition - startPoint, view.HeadRotation * Vector3.up),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var finalPoint = attach && attachMode == AttachMode.RotateAroundTarget
                ? targetStartPosition + view.HeadRotation * offset
                : targetStartPosition + offsetOrient * offset;
                    
            var rot = Quaternion.LookRotation(offsetOrient * offset, Vector3.up);
            var curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, finalPoint, rot, curvature);
            
            var startPointOffset = startPoint - targetStartPosition;
            var curvePointOffset = curvePoint - targetStartPosition;
            var finalPointOffset = finalPoint - targetStartPosition;
            
            float pathLength = BezierExtensions.GetBezier3PointsLength(startPoint, curvePoint, finalPoint);
            float speed = pathLength > 0f ? this.speed / pathLength : float.MaxValue;
            float t = 0f;
            
            while (!cancellationToken.IsCancellationRequested) {
                target.GetPositionAndRotation(out var targetPosition, out var targetRotation);
                
                var targetRotationOffset = attach && attachMode == AttachMode.RotateAroundTarget
                    ? view.HeadRotation * Quaternion.Inverse(headStartRotation) 
                    : targetRotation * Quaternion.Inverse(targetStartRotation);
                
                startPoint = targetPosition + targetRotationOffset * startPointOffset;
                curvePoint = targetPosition + targetRotationOffset * curvePointOffset;
                finalPoint = targetPosition + targetRotationOffset * finalPointOffset;

                var diff = finalPoint - view.HeadPosition;

                float dt = UnityEngine.Time.deltaTime;
                float k = reduceSpeedBelowDistance > 0f ? Mathf.Clamp01(diff.magnitude / reduceSpeedBelowDistance) : 1f;
                t = Mathf.Clamp01(t + dt * Mathf.Max(speed * k, speedMin));

                var position = BezierExtensions.EvaluateBezier3Points(
                    startPoint,
                    curvePoint,
                    finalPoint,
                    progressCurve.Evaluate(t)
                );

                view.HeadPosition = position;
                
#if UNITY_EDITOR
                DebugExt.DrawSphere(view.HeadPosition, 0.008f, Color.yellow, duration: 5f);
#endif
                
                if (t >= 1f) break;
                
                await UniTask.Yield();
            }
            
#if UNITY_EDITOR
            DebugExt.DrawSphere(view.HeadPosition, 0.01f, Color.green, duration: 5f);
#endif
            
            if (cancellationToken.IsCancellationRequested) return;
            
            if (attach) view.AttachTo(target, view.HeadPosition, attachMode, attachSmoothing);
        }
    }
    
}