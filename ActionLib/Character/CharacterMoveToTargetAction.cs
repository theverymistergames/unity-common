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
        [Min(0f)] public float speed;
        public float curvature;
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
            float duration = Mathf.Max(pathLength / speed, 0f);
            
#if UNITY_EDITOR
            DebugExt.DrawBezier3Points(startPoint, curvePoint, targetPoint, Color.yellow, duration: duration);
#endif

            collisions.enabled = false;
            float t = 0f;

            while (!cancellationToken.IsCancellationRequested) {
                t = duration > 0f ? Mathf.Clamp01(t + UnityEngine.Time.deltaTime / duration) : 1f;
                float progress = progressCurve.Evaluate(t);

                body.Position = BezierExtensions.EvaluateBezier3Points(startPoint, curvePoint, targetPoint, progress);
                
                if (t >= 1f) break;

                await UniTask.Yield();
            }

            if (cancellationToken.IsCancellationRequested) return;
            
            collisions.enabled = true;
        }
    }
    
}