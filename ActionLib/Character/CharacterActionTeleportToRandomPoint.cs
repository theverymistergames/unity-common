using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionTeleportToRandomPoint : IActorAction {
        
        public bool preserveVelocity;
        public Transform[] points;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (points.Length == 0) return default;
            
            points.GetRandom().GetPositionAndRotation(out var pos, out var rot);
            context.GetComponent<CharacterMotionPipeline>().Teleport(pos, rot, preserveVelocity);
            
            return default;
        }
    }
    
}