using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Phys {
    
    [Serializable]
    public sealed class SetRigidbodyParamsAction : IActorAction {

        public Rigidbody rigidbody;
        public Optional<bool> isKinematic;
        public Optional<bool> useGravity;
        public Optional<RigidbodyInterpolation> interpolation;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (isKinematic.HasValue) rigidbody.isKinematic = isKinematic.Value;
            if (useGravity.HasValue) rigidbody.useGravity = useGravity.Value;
            if (interpolation.HasValue) rigidbody.interpolation = interpolation.Value;

            return default;
        }
    }
    
}