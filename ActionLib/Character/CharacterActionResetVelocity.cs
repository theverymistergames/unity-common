using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionResetVelocity : IActorAction {

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            context
                .GetComponent<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>()
                .ApplyVelocityChange(Vector3.zero);
            
            return default;
        }
    }
    
}
