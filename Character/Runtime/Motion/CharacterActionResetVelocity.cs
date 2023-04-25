using System;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Actions;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionResetVelocity : ICharacterAction {

        public void Apply(object source, ICharacterAccess characterAccess) {
            characterAccess
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>()
                .ApplyVelocityChange(Vector3.zero);
        }
    }
    
}
