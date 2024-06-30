using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedMultiplier : IActorAction {

        [Min(0f)] public float speed;
        [Min(0f)] public float moveForce;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var motion = context.GetComponent<CharacterMotionPipeline>();
            motion.Speed = speed;
            motion.MoveForce = moveForce;
            return default;
        }
    }
    
}
