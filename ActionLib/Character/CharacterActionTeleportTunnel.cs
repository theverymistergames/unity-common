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
            
            var collisionPipeline = context.GetComponent<CharacterCollisionPipeline>();

            collisionPipeline.enabled = false;

            var localForward = localCenter.forward;
            var targetForward = targetCenter.forward;
            var positionOffset = body.Position - localCenter.position;
            
            float angle = Vector3.SignedAngle(localForward, targetForward, body.Rotation * Vector3.up);
            
            body.Position = targetCenter.position + Quaternion.FromToRotation(localForward, targetForward) * positionOffset;
            body.Rotation *= Quaternion.Euler(0f, angle, 0f);
            
            collisionPipeline.enabled = true;

            return default;
        }
    }
    
}