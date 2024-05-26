using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Collisions;
using MisterGames.Character.Motion;
using MisterGames.Common;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterMoveToTargetAction : IActorAction {

        public Transform target;
        public Vector3 offset;
        public Vector3 rotationOffset;
        public OffsetMode offsetMode;
        [Min(0f)] public float speed = 1f;
        [Min(0f)] public float reduceSpeedBelowDistance = 0.1f;
        [Min(0f)] public float pointRadius = 0.1f;
        [Min(0f)] public float smoothing = 10f;
        public float curvature = 1f;
        public bool allowTranslationY;
        public AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public enum OffsetMode {
            Local,
            World,
        }
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var body = context.GetComponent<CharacterBodyAdapter>();
            var collisions = context.GetComponent<CharacterCollisionPipeline>();
            
            var targetRotation =
                (offsetMode == OffsetMode.Local ? target.rotation : Quaternion.identity) * 
                Quaternion.Euler(rotationOffset);
            
            var targetPoint = target.position + targetRotation * offset;
            var startPoint = body.Position;

            if (!allowTranslationY) targetPoint.y = startPoint.y;
            
            var curvePoint = BezierExtensions.GetCurvaturePoint(startPoint, targetPoint, targetRotation, curvature);
            float pathLength = BezierExtensions.GetBezier3PointsLength(startPoint, curvePoint, targetPoint);
            float speed = pathLength > 0f ? this.speed / pathLength : float.MaxValue;

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
                
                body.Position = smoothing > 0f 
                    ? Vector3.Lerp(body.Position, position, smoothing * dt)
                    : position;

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