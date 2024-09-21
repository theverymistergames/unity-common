using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class SpawnPrefabAction : IActorAction {
        
        public GameObject prefab;
        public Mode mode;
        [VisibleIf(nameof(mode), 1)]
        public Transform explicitTransform;
        public Vector3 offset;
        public Vector3 rotationOffset;
        public bool parentToTransform;

        public enum Mode {
            UseActorTransform,
            UseExplicitTransform
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var root = mode switch {
                Mode.UseActorTransform => context.Transform,
                Mode.UseExplicitTransform => explicitTransform,
                _ => throw new ArgumentOutOfRangeException()
            };

            root.GetPositionAndRotation(out var pos, out var rot);
            var parent = parentToTransform ? root : PrefabPool.Main.ActiveSceneRoot;

            PrefabPool.Main.Get(prefab, pos + offset, rot * Quaternion.Euler(rotationOffset), parent);
            return default;
        }
    }
    
}