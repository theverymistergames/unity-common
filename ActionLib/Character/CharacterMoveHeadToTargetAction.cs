﻿using System;
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
        [VisibleIf(nameof(targetType), 1)] [Min(0f)] public float pointRadius = 0.05f;
        [VisibleIf(nameof(targetType), 1)] [Min(0f)] public float smoothing = 10f;
        
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
            var head = context.GetComponent<CharacterHeadAdapter>();
            var body = context.GetComponent<CharacterBodyAdapter>();
            var view = context.GetComponent<CharacterViewPipeline>();
            
            Vector3 startPoint;
            Vector3 targetPoint;
            Vector3 curvePoint;
            Vector3 dest;
            
            var targetRotation = Quaternion.identity;
            var offsetOrient = Quaternion.identity;
            bool invertCurve = false;
            float offsetDist = offset.magnitude;
            
            switch (targetType) {
                case TargetType.LocalPosition: {
                    if (useTargetAsCurvePointOrigin) {
                        startPoint = localPosition;
                        targetPoint = head.LocalPosition;
                        var rot = target.rotation * Quaternion.Euler(rotationOffset) * Quaternion.Inverse(body.Rotation);
                        curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, rot, curvature);
                        invertCurve = true;
                    }
                    else {
                        startPoint = head.LocalPosition;
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
                    targetRotation = target.rotation;
                    startPoint = head.Position;
                    var targetPos = target.position;
                    
                    offsetOrient = offsetMode switch {
                        OffsetMode.Local => targetRotation * Quaternion.Euler(rotationOffset),
                        OffsetMode.World => Quaternion.Euler(rotationOffset),
                        OffsetMode.UseViewDirectionAsForward => Quaternion.LookRotation(targetPos - startPoint, head.Rotation * Vector3.up),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    
                    targetPoint = attach && attachMode == AttachMode.RotateAroundTarget
                        ? targetPos + Quaternion.Euler(view.CurrentOrientation) * new Vector3(0f, 0f, -offsetDist)
                        : targetPos + offsetOrient * offset;
                    
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
                    var p = head.Position;
                    DebugExt.DrawSphere(head.LocalPosition, 0.03f, Color.green, duration: 5f);
                    head.LocalPosition = curvePoint;
                    DebugExt.DrawSphere(head.Position, 0.03f, Color.yellow, duration: 5f);
                    head.LocalPosition = dest;
                    DebugExt.DrawSphere(head.Position, 0.03f, Color.red, duration: 5f);
                    head.Position = p;
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
                        diff = dest - head.LocalPosition;
                        break;
                    
                    case TargetType.Transform:
                        dest = attach && attachMode == AttachMode.RotateAroundTarget
                            ? target.position + Quaternion.Euler(view.CurrentOrientation) * new Vector3(0f, 0f, -offsetDist)
                            : target.position + offsetOrient * (Quaternion.Inverse(targetRotation) * target.rotation) * offset;
                        
                        targetPoint = dest;
                        diff = dest - head.Position;
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

                bool canFinish;

                switch (targetType) {
                    case TargetType.LocalPosition:
                        head.LocalPosition = position;
                        canFinish = t >= 1f;
                        break;
                    
                    case TargetType.Transform:
                        head.Position = smoothing > 0f 
                            ? Vector3.Lerp(head.Position, position, smoothing * dt) 
                            : position;
                        float r = Mathf.Max(pointRadius, Mathf.Epsilon);
                        canFinish = t >= 1f && (dest - head.Position).sqrMagnitude <= r * r;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
#if UNITY_EDITOR
                DebugExt.DrawSphere(head.Position, 0.008f, Color.yellow, duration: 5f);
                DebugExt.DrawSphere(dest, 0.01f, Color.green, duration: 5f);
#endif
                
                if (canFinish) break;

                await UniTask.Yield();
            }
            
            if (cancellationToken.IsCancellationRequested) return;

            if (targetType == TargetType.Transform && attach) view.Attach(target, dest, attachMode, attachSmoothing);
        }
    }
    
}