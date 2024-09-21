using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class SpawnPrefabInCircleAction : IActorAction {
        
        [Header("Prefab Settings")]
        public GameObject prefab;
        public Mode mode;
        [VisibleIf(nameof(mode), 1)]
        public Transform explicitTransform;
        public Vector3 offset;
        public Vector3 rotationOffset;
        public bool parentToTransform;

        [Header("Radial Settings")]
        [Min(0)] public int spawnCount;
        public Vector3 axis = Vector3.up;
        public float radius;
        public float startAngle;
        public float endAngle;
        [Min(0f)] public float stepDelay;

        public enum Mode {
            UseActorTransform,
            UseExplicitTransform
        }
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default)
        {
            var root = mode switch {
                Mode.UseActorTransform => context.Transform,
                Mode.UseExplicitTransform => explicitTransform,
                _ => throw new ArgumentOutOfRangeException()
            };

            var parent = parentToTransform ? root : PrefabPool.Main.ActiveSceneRoot;

            for (int i = 0; i < spawnCount && !cancellationToken.IsCancellationRequested; i++) {
                float t = spawnCount == 1 ? 0.5f : (float) i / (spawnCount - 1);
                float angle = Mathf.Lerp(startAngle, endAngle, t);

                root.GetPositionAndRotation(out var pos, out var rot);
                
                pos += offset;
                rot *= Quaternion.Euler(rotationOffset);
                
                var p = pos + rot * Quaternion.AngleAxis(angle, axis) * Vector3.forward * radius;
                
                PrefabPool.Main.Get(prefab, p, rot, parent);

#if UNITY_EDITOR
                DebugExt.DrawSphere(p, 0.1f, Color.green, duration: 5f);          
#endif

                if (stepDelay > 0f) {
                    await UniTask.Delay(TimeSpan.FromSeconds(stepDelay), cancellationToken: cancellationToken)
                        .SuppressCancellationThrow();    
                }
            }
        }
    }
    
}