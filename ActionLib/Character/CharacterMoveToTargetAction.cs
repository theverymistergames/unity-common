using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Collisions;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterMoveToTargetAction : IActorAction {
        
        public TargetType targetType;
        [VisibleIf(nameof(targetType), 0)] public Transform target;
        public Vector3 offset;
        public Vector3 rotationOffset;
        public OffsetMode offsetMode;
        [Min(0f)] public float speed = 1f;
        [Min(0f)] public float reduceSpeedBelowDistance = 0.1f;
        [Min(0f)] public float pointRadius = 0.1f;
        [Min(0f)] public float smoothing = 10f;
        public float curvature = 1f;
        public bool allowTranslationY;
        public bool saveHeadPosition;
        public bool detectGround;
        [VisibleIf(nameof(detectGround))] public LayerMask layer;
        [VisibleIf(nameof(detectGround))] public float rayElevation;
        [VisibleIf(nameof(detectGround))] public float maxDistance;
        [VisibleIf(nameof(detectGround))] public float groundOffset;
        public AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public enum TargetType {
            Transform,
            Head
        }
        
        public enum OffsetMode {
            Local,
            World,
        }
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var body = context.GetComponent<CharacterBodyAdapter>();
            var head = context.GetComponent<CharacterHeadAdapter>();
            var collisions = context.GetComponent<CharacterCollisionPipeline>();

            var rot = targetType switch {
                TargetType.Transform => target.rotation,
                TargetType.Head => head.Rotation,
                _ => Quaternion.identity,
            };
            
            var targetRotation =
                (offsetMode == OffsetMode.Local ? rot : Quaternion.identity) * 
                Quaternion.Euler(rotationOffset);
            
            var targetPoint = targetType switch {
                TargetType.Transform => target.position + targetRotation * offset,
                TargetType.Head => head.Position + targetRotation * offset,
                _ => body.Position,
            };

            if (detectGround) {
                bool hasHit = Physics.Raycast(
                    targetPoint + Vector3.up * rayElevation,
                    Vector3.down,
                    out var hit,
                    maxDistance,
                    layer,
                    QueryTriggerInteraction.Ignore
                );

                if (hasHit) {
                    targetPoint.y = (hit.point + Vector3.up * groundOffset).y;
                }
            }
            
            var startPoint = body.Position;

            if (!allowTranslationY) targetPoint.y = startPoint.y;
            
            var curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, targetRotation, curvature);
            float pathLength = BezierExtensions.GetBezier3PointsLength(startPoint, curvePoint, targetPoint);
            float speed = pathLength > 0f && this.speed > 0f ? this.speed / pathLength : float.MaxValue;

            collisions.enabled = false;
            float t = 0f;

            while (!cancellationToken.IsCancellationRequested) {
                float distance = (targetPoint - body.Position).magnitude;
                float dt = UnityEngine.Time.deltaTime;
                float k = reduceSpeedBelowDistance > 0f ? Mathf.Clamp01(distance / reduceSpeedBelowDistance) : 1f;

                t = Mathf.Clamp01(t + speed * k * dt);
                
                var position = BezierExtensions.EvaluateBezier3Points(
                    startPoint,
                    curvePoint,
                    targetPoint,
                    progressCurve.Evaluate(t)
                );

                var headPosition = head.Position;
                
                body.Position = smoothing > 0f 
                    ? Vector3.Lerp(body.Position, position, smoothing * dt)
                    : position;

                if (saveHeadPosition) head.Position = headPosition;
                
                float r = Mathf.Max(pointRadius, Mathf.Epsilon);
                if ((targetPoint - body.Position).sqrMagnitude <= r * r && t >= 1f) break;
                
#if UNITY_EDITOR
                DebugExt.DrawSphere(body.Position, 0.005f, Color.yellow, duration: 5f);
#endif

                await UniTask.Yield();
            }

            if (cancellationToken.IsCancellationRequested) return;
            
            collisions.enabled = true;
        }
    }
    
}