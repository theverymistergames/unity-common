using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Collisions;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionTeleportTunnel : IActorAction {
        
        public Transform localCenter;
        public Transform targetCenter;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var collisionPipeline = context.GetComponent<CharacterCollisionPipeline>();
            var rb = context.GetComponent<Rigidbody>();
            var view = context.GetComponent<CharacterViewPipeline>();
            
            var t = rb.transform;
            
            t.GetPositionAndRotation(out var pos, out var rot);
            
            collisionPipeline.enabled = false;

            var velocity = rb.velocity;
            
            rb.isKinematic = true;
            var interpolation = rb.interpolation;
            rb.interpolation = RigidbodyInterpolation.None;
            
            var localForward = localCenter.forward;
            var targetForward = targetCenter.forward;
            var positionOffset = pos - localCenter.position;
            
            float angle = Vector3.SignedAngle(localForward, targetForward, rot * Vector3.up);
            var rotOffset = Quaternion.FromToRotation(localForward, targetForward);
            var rotDelta = Quaternion.Euler(0f, angle, 0f);
            
            t.position = targetCenter.position + rotOffset * positionOffset;
            t.rotation *= rotDelta;
            view.Rotation *= rotDelta;
            
            collisionPipeline.enabled = true;
            rb.isKinematic = false;
            rb.interpolation = interpolation;
            rb.velocity = rotOffset * velocity;
            
            return default;
        }
    }
    
}