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

        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var view = context.GetComponent<CharacterViewPipeline>();

            Vector3 startPoint;
            Vector3 targetPoint;
            Vector3 curvePoint;
            Vector3 dest;
            
            var targetPosAtStart = Vector3.zero;
            var targetRotAtStart = Quaternion.identity;
            
            var targetPosOffset = Vector3.zero;
            
            var offsetOrient = Quaternion.identity;
            bool invertCurve = false;
            float offsetDist = offset.magnitude;
            
            switch (targetType) {
                case TargetType.LocalPosition: {
                    if (useTargetAsCurvePointOrigin) {
                        startPoint = localPosition;
                        targetPoint = view.LocalPosition;
                        var rot = target.rotation * Quaternion.Euler(rotationOffset) * Quaternion.Inverse(view.BodyRotation);
                        curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, rot, curvature);
                        invertCurve = true;
                    }
                    else {
                        startPoint = view.LocalPosition;
                        targetPoint = localPosition;
                        var rot = 
                            Quaternion.LookRotation(targetPoint.sqrMagnitude > 0f ? -targetPoint : startPoint, Vector3.up) * 
                            Quaternion.Euler(rotationOffset);
                        
                        curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, rot, curvature);
                    }
                    
                    dest = localPosition;
                    break;
                }

                case TargetType.Transform: {
                    targetRotAtStart = target.rotation;
                    targetPosAtStart = target.position;
                    
                    startPoint = view.Position;

                    offsetOrient = offsetMode switch {
                        OffsetMode.Local => targetRotAtStart * Quaternion.Euler(rotationOffset),
                        OffsetMode.World => Quaternion.Euler(rotationOffset),
                        OffsetMode.UseViewDirectionAsForward => Quaternion.LookRotation(targetPosAtStart - startPoint, view.Rotation * Vector3.up),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    
                    targetPoint = attach && attachMode == AttachMode.RotateAroundTarget
                        ? targetPosAtStart + Quaternion.Euler(view.Rotation.ToEulerAngles180()) * new Vector3(0f, 0f, -offsetDist)
                        : targetPosAtStart + offsetOrient * offset;
                    
                    var rot = Quaternion.LookRotation(offsetOrient * offset, Vector3.up);
                    curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, rot, curvature);
                    
                    dest = targetPoint;
                    break;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
#if UNITY_EDITOR
            switch (targetType) {
                case TargetType.LocalPosition:
                    var p = view.Position;
                    DebugExt.DrawSphere(view.LocalPosition, 0.03f, Color.green, duration: 5f);
                    view.LocalPosition = curvePoint;
                    DebugExt.DrawSphere(view.Position, 0.03f, Color.yellow, duration: 5f);
                    view.LocalPosition = dest;
                    DebugExt.DrawSphere(view.Position, 0.03f, Color.red, duration: 5f);
                    view.Position = p;
                    break;
                
                case TargetType.Transform:
                    DebugExt.DrawSphere(startPoint, 0.03f, Color.green, duration: 5f);
                    DebugExt.DrawSphere(curvePoint, 0.03f, Color.yellow, duration: 5f);
                    DebugExt.DrawSphere(targetPoint, 0.03f, Color.red, duration: 5f);
                    break;
            }
#endif
            
            float pathLength = BezierExtensions.GetBezier3PointsLength(startPoint, curvePoint, targetPoint);
            float speed = pathLength > 0f ? this.speed / pathLength : float.MaxValue;
            float t = 0f;
            
            while (!cancellationToken.IsCancellationRequested) {
                Vector3 diff;
                
                switch (targetType) {
                    case TargetType.LocalPosition:
                        diff = dest - view.LocalPosition;
                        break;
                    
                    case TargetType.Transform:
                        var targetPos = target.position;
                        
                        targetPosOffset = targetPos - targetPosAtStart;
                        
                        dest = attach && attachMode == AttachMode.RotateAroundTarget
                            ? targetPosAtStart + Quaternion.Euler(view.Rotation.ToEulerAngles180()) * new Vector3(0f, 0f, -offsetDist)
                            : targetPosAtStart + offsetOrient * (Quaternion.Inverse(targetRotAtStart) * target.rotation) * offset;

                        diff = dest - view.Position;
                        targetPoint = dest;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                float dt = UnityEngine.Time.deltaTime;
                float k = reduceSpeedBelowDistance > 0f ? Mathf.Clamp01(diff.magnitude / reduceSpeedBelowDistance) : 1f;
                t = Mathf.Clamp01(t + dt * Mathf.Max(speed * k, speedMin));

                var position = BezierExtensions.EvaluateBezier3Points(
                    startPoint,
                    curvePoint,
                    targetPoint,
                    invertCurve ? 1f - progressCurve.Evaluate(t) : progressCurve.Evaluate(t)
                );

                switch (targetType) {
                    case TargetType.LocalPosition:
                        view.LocalPosition = position;
                        break;
                    
                    case TargetType.Transform:
                        view.Position = position + targetPosOffset;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
#if UNITY_EDITOR
                DebugExt.DrawSphere(view.Position, 0.008f, Color.yellow, duration: 5f);
#endif
                
                if (t >= 1f) break;

                await UniTask.Yield();
            }
            
            if (cancellationToken.IsCancellationRequested) return;

            if (targetType == TargetType.Transform && attach) view.AttachTo(target, dest + targetPosOffset, attachMode, attachSmoothing);
        }
    }
    
}