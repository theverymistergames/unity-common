using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Actions;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionResetVelocity : ICharacterAction {

        public UniTask Apply(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            characterAccess
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>()
                .ApplyVelocityChange(Vector3.zero);

            return default;
        }
    }
    
}
