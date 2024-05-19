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
        
        [Header("Motion")]
        [Min(0f)] public float speed;
        public float curvature;
        public AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        public enum TargetType {
            LocalPosition,
            Transform
        }
        
        public enum OffsetMode {
            Local,
            World,
            UseViewDirectionAsForward
        }
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var head = context.GetComponent<CharacterHeadAdapter>();
            var body = context.GetComponent<CharacterBodyAdapter>();
            
            Vector3 startPoint;
            Vector3 targetPoint;
            Vector3 curvePoint;
            bool invertCurve = false;
            
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
                    
                    break;
                }

                case TargetType.Transform: {
                    startPoint = head.Position;
                    var targetPos = target.position;
                    
                    var offsetOrient = offsetMode switch {
                        OffsetMode.Local => target.rotation * Quaternion.Euler(rotationOffset),
                        OffsetMode.World => Quaternion.Euler(rotationOffset),
                        OffsetMode.UseViewDirectionAsForward => Quaternion.LookRotation(targetPos - startPoint, head.Rotation * Vector3.up),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    
                    targetPoint = targetPos + offsetOrient * offset;
                    
                    var targetRotation = Quaternion.LookRotation(offsetOrient * offset, Vector3.up);
                    curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, targetRotation, curvature);
                    
                    break;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            float pathLength = BezierExtensions.GetBezier3PointsLength(startPoint, curvePoint, targetPoint);
            float duration = speed > 0f ? Mathf.Max(pathLength / speed, 0f) : 0f;
            float t = 0f;
            
            while (!cancellationToken.IsCancellationRequested) {
                t = duration > 0f ? Mathf.Clamp01(t + UnityEngine.Time.deltaTime / duration) : 1f;

                var p = BezierExtensions.EvaluateBezier3Points(
                    startPoint,
                    curvePoint,
                    targetPoint,
                    invertCurve ? 1f - progressCurve.Evaluate(t) : progressCurve.Evaluate(t)
                );

                switch (targetType) {
                    case TargetType.LocalPosition:
                        head.LocalPosition = p;
                        break;
                    
                    case TargetType.Transform:
                        head.Position = p;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
#if UNITY_EDITOR
                DebugExt.DrawSphere(head.Position, 0.005f, Color.yellow, duration: duration);
#endif
                
                if (t >= 1f) break;

                await UniTask.Yield();
            }
        }
    }
    
}