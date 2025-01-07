using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class SpawnPrefabAction : IActorAction {
        
        public GameObject prefab;
        public Mode mode;
        [VisibleIf(nameof(mode), 1)]
        public Transform explicitTransform;
        public Inherit inherit = Inherit.Position | Inherit.Rotation;
        public Vector3 offset;
        public Vector3 rotationOffset;
        public Optional<Vector3> scale = Optional<Vector3>.WithDisabled(Vector3.one);
        public bool parentToTransform;

        public enum Mode {
            UseActorTransform,
            UseExplicitTransform,
        }

        [Flags]
        public enum Inherit {
            None = 0,
            Position = 1,
            Rotation = 2,
            Scale = 4,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var root = mode switch {
                Mode.UseActorTransform => context.Transform,
                Mode.UseExplicitTransform => explicitTransform,
                _ => throw new ArgumentOutOfRangeException()
            };

            root.GetPositionAndRotation(out var rootPos, out var rootRot);
            var parent = parentToTransform ? root : PrefabPool.Main.ActiveSceneRoot; 
            
            rootPos = (inherit & Inherit.Position) == Inherit.Position ? rootPos : Vector3.zero;
            rootRot = (inherit & Inherit.Rotation) == Inherit.Rotation ? rootRot : Quaternion.identity;
            var rootScale = (inherit & Inherit.Scale) == Inherit.Scale ? root.localScale : Vector3.one;
            
            PrefabPool.Main
                .Get(prefab, rootPos + offset, rootRot * Quaternion.Euler(rotationOffset), parent)
                .transform.localScale = (scale.HasValue ? scale.Value : prefab.transform.localScale).Multiply(rootScale);
            
            return default;
        }
    }
    
}