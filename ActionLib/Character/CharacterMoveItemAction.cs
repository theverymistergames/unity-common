using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterMoveItemAction : IActorAction {

        [Header("Target")]
        public Transform item;
        public TargetType targetType;
        [VisibleIf(nameof(targetType), 1)] public Transform target;
        public OffsetMode offsetMode;
        public Vector3 offset;
        public Vector3 rotationOffset;
        public Vector3 itemRotation;
        
        [Header("Start")]
        public bool disableColliderOnStart;
        public bool detachOnStart;
        
        [Header("Finish")]
        public bool enableColliderOnFinish;
        public bool attachOnFinish;
        
        [Header("Motion")]
        [Min(0f)] public float speed = 1f;
        [Min(0f)] public float reduceSpeedBelowDistance = 0.5f;
        [Min(0f)] public float speedMin = 0.01f;
        public float curvature = 1f;
        public AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        public enum TargetType {
            Head,
            Transform,
        }

        public enum OffsetMode {
            Local,
            World,
            UseViewDirectionAsForward,
        }

        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var head = context.GetComponent<CharacterHeadAdapter>();
            var view = context.GetComponent<CharacterViewPipeline>();

            if (detachOnStart) view.DetachObject(item);
            
            var startPoint = item.position;
            var startRotation = item.rotation;
            Vector3 targetPoint;
            Vector3 curvePoint;

            var collider = item.gameObject.GetComponent<Collider>();
            if (disableColliderOnStart && collider != null) collider.enabled = false;

            switch (targetType) {
                case TargetType.Head: {
                    var offsetOrient = offsetMode switch {
                        OffsetMode.Local => head.Rotation * Quaternion.Euler(rotationOffset),
                        OffsetMode.World => Quaternion.Euler(rotationOffset),
                        OffsetMode.UseViewDirectionAsForward => Quaternion.LookRotation(startPoint - head.Position, head.Rotation * Vector3.up),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    
                    targetPoint = head.Position + offsetOrient * offset;
                    curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, head.Rotation, curvature);
                    break;
                }

                case TargetType.Transform: {
                    var targetPos = target.position;
                    var targetRot = target.rotation;

                    var offsetOrient = offsetMode switch {
                        OffsetMode.Local => targetRot * Quaternion.Euler(rotationOffset),
                        OffsetMode.World => Quaternion.Euler(rotationOffset),
                        OffsetMode.UseViewDirectionAsForward => Quaternion.LookRotation(startPoint - targetPos, head.Rotation * Vector3.up),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    
                    targetPoint = targetPos + offsetOrient * offset;
                    curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, targetRot, curvature);
                    break;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            float pathLength = BezierExtensions.GetBezier3PointsLength(startPoint, curvePoint, targetPoint);
            float speed = pathLength > 0f ? this.speed / pathLength : float.MaxValue;
            float t = 0f;
            
            while (!cancellationToken.IsCancellationRequested) {
                Vector3 diff;
                Quaternion targetRotation;
                
                switch (targetType) {
                    case TargetType.Head: {
                        var offsetOrient = offsetMode switch {
                            OffsetMode.Local => head.Rotation * Quaternion.Euler(rotationOffset),
                            OffsetMode.World => Quaternion.Euler(rotationOffset),
                            OffsetMode.UseViewDirectionAsForward => Quaternion.LookRotation(startPoint - head.Position, head.Rotation * Vector3.up),
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        targetPoint = head.Position + offsetOrient * offset;
                        targetRotation = head.Rotation * Quaternion.Euler(itemRotation);
                        diff = targetPoint - item.position;
                        break;
                    }

                    case TargetType.Transform: {
                        var targetPos = target.position;
                        var targetRot = target.rotation;

                        var offsetOrient = offsetMode switch {
                            OffsetMode.Local => targetRot * Quaternion.Euler(rotationOffset),
                            OffsetMode.World => Quaternion.Euler(rotationOffset),
                            OffsetMode.UseViewDirectionAsForward => Quaternion.LookRotation(startPoint - targetPos, head.Rotation * Vector3.up),
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        targetPoint = targetPos + offsetOrient * offset;
                        targetRotation = targetRot * Quaternion.Euler(itemRotation);
                        diff = targetPoint - item.position;
                        break;
                    }
                    
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
                    progressCurve.Evaluate(t)
                );

                item.position = position;
                item.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationCurve.Evaluate(t));

#if UNITY_EDITOR
                DebugExt.DrawSphere(item.position, 0.008f, Color.yellow, duration: 5f);
#endif
                
                if (t >= 1f) break;

                await UniTask.Yield();
            }
            
            if (cancellationToken.IsCancellationRequested) return;

            if (enableColliderOnFinish && collider != null) collider.enabled = true;
            if (attachOnFinish) view.AttachObject(item, item.position);
        }
    }
    
}