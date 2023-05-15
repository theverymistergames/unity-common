using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Actions;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionGravityEnableDisable : ICharacterAction {

        public bool isEnabled;

        public UniTask Apply(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var mass = characterAccess
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>();

            mass.isGravityEnabled = isEnabled;
            return default;
        }
    }
    
}
