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
        [VisibleIf(nameof(targetType), 0)] public bool useBodyRotation;
        [VisibleIf(nameof(targetType), 1)] public Transform target;
        public OffsetMode offsetMode;
        public Vector3 offset;
        public Vector3 rotationOffset;
        public Vector3 itemRotation;
        
        [Header("Motion")]
        [Min(0f)] public float speed = 1f;
        public float curvature = 1f;
        public AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        [Header("Attach")]
        public bool detachOnStart;
        public bool attachOnFinish;
        [Min(0f)] public float attachSmoothing;
        
        [Header("Collider Options")]
        public bool disableColliderOnStart;
        public bool enableColliderOnFinish;
        
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
            var view = context.GetComponent<CharacterViewPipeline>();
            
            if (detachOnStart) view.DetachObject(item);
            
            Vector3 targetStartPosition;
            Quaternion targetStartRotation;
            
            var collider = item.gameObject.GetComponent<Collider>();
            if (disableColliderOnStart && collider != null) collider.enabled = false;

            switch (targetType) {
                case TargetType.Head: {
                    targetStartPosition = view.HeadPosition;
                    targetStartRotation = useBodyRotation ? view.BodyRotation : view.HeadRotation;
                    break;
                }

                case TargetType.Transform: {
                    target.GetPositionAndRotation(out targetStartPosition, out targetStartRotation);
                    break;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            item.GetPositionAndRotation(out var startPoint, out var startRotation);
            
            var offsetOrient = offsetMode switch {
                OffsetMode.Local => targetStartRotation * Quaternion.Euler(rotationOffset),
                OffsetMode.World => Quaternion.Euler(rotationOffset),
                OffsetMode.UseViewDirectionAsForward => Quaternion.LookRotation(startPoint - targetStartPosition, view.HeadRotation * Vector3.up),
                _ => throw new ArgumentOutOfRangeException()
            };
                    
            var finalPoint = targetStartPosition + offsetOrient * offset;
            var curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, finalPoint, targetStartRotation, curvature);
            var finalRotation = view.HeadRotation * Quaternion.Euler(itemRotation);

            var startPointOffset = startPoint - targetStartPosition;
            var curvePointOffset = curvePoint - targetStartPosition;
            var finalPointOffset = finalPoint - targetStartPosition;
            
            var startRotationOffset = Quaternion.Inverse(targetStartRotation) * startRotation;
            var finalRotationOffset = Quaternion.Inverse(targetStartRotation) * finalRotation;

            float pathLength = BezierExtensions.GetBezier3PointsLength(startPoint, curvePoint, finalPoint);
            float speed = pathLength > 0f && this.speed > 0f ? this.speed / pathLength : float.MaxValue;
            float t = 0f;
            
            while (!cancellationToken.IsCancellationRequested) {
                Vector3 targetPosition;
                Quaternion targetRotation;
                
                switch (targetType) { 
                    case TargetType.Head: {
                        targetPosition = view.HeadPosition;
                        targetRotation = useBodyRotation ? view.BodyRotation : view.HeadRotation;
                        break;
                    }
                     
                    case TargetType.Transform: {
                        target.GetPositionAndRotation(out targetPosition, out targetRotation);
                        break;
                    }
                     
                    default: 
                        throw new ArgumentOutOfRangeException(); 
                }

                var targetRotationOffset = targetRotation * Quaternion.Inverse(targetStartRotation);

                startPoint = targetPosition + targetRotationOffset * startPointOffset;
                curvePoint = targetPosition + targetRotationOffset * curvePointOffset;
                finalPoint = targetPosition + targetRotationOffset * finalPointOffset;

                startRotation = targetRotation * startRotationOffset;
                finalRotation = targetRotation * finalRotationOffset;

                t = Mathf.Clamp01(t + UnityEngine.Time.deltaTime * speed);

                var position = BezierExtensions.EvaluateBezier3Points(
                    startPoint,
                    curvePoint,
                    finalPoint,
                    progressCurve.Evaluate(t)
                );
                
                item.position = position;
                item.rotation = Quaternion.Slerp(startRotation, finalRotation, rotationCurve.Evaluate(t));

                if (t >= 1f) break;
                
#if UNITY_EDITOR
                DebugExt.DrawSphere(item.position, 0.008f, Color.yellow, duration: 5f);
#endif
                
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            }
            
#if UNITY_EDITOR
            DebugExt.DrawSphere(item.position, 0.01f, Color.green, duration: 5f);
#endif
            
            if (cancellationToken.IsCancellationRequested) return;

            if (enableColliderOnFinish && collider != null) collider.enabled = true;

            if (attachOnFinish) view.AttachObject(item, item.position, attachSmoothing);
        }
    }
    
}