using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Collisions;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionTeleportTunnel : IActorAction {
        
        public Transform localCenter;
        public Transform targetCenter;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var body = context.GetComponent<CharacterBodyAdapter>();
            
            var collisionPipeline = context.GetComponent<ICharacterCollisionPipeline>();

            collisionPipeline.IsEnabled = false;

            var fwd = localCenter.forward;
            var positionOffset = body.Position - localCenter.position;
            var rotationOffset = Quaternion.FromToRotation(fwd, body.Rotation * Vector3.forward);
            
            body.Position = targetCenter.position + Quaternion.FromToRotation(fwd, targetCenter.forward) * positionOffset;
            body.Rotation = targetCenter.rotation * rotationOffset;
            
            collisionPipeline.IsEnabled = true;

            return default;
        }
    }
    
}