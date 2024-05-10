using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionResetVelocity : IActorAction {

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context
                .GetComponent<CharacterMotionPipeline>()
                .GetProcessor<CharacterMassProcessor>()
                .ApplyVelocityChange(Vector3.zero);
            
            return default;
        }
    }
    
}
