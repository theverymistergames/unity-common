using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionResetVelocity : ICharacterAction {

        public UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default) {
            var mass = characterAccess
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>();

            mass.ApplyVelocityChange(Vector3.zero);
            return default;
        }
    }
    
}
