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
            var motion = context.GetComponent<CharacterMotionPipeline>();
            context.Transform.GetPositionAndRotation(out var pos, out var rot);

            var positionOffset = pos - localCenter.position;
            var rotOffset = targetCenter.rotation * Quaternion.Inverse(localCenter.rotation);
            
            motion.Teleport(targetCenter.position + rotOffset * positionOffset, rotOffset * rot);
            
            return default;
        }
    }
    
}