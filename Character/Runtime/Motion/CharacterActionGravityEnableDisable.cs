using System;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Actions;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionGravityEnableDisable : ICharacterAction {

        public bool isEnabled;

        public void Apply(object source, ICharacterAccess characterAccess) {
            var mass = characterAccess
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorMass>();

            mass.isGravityEnabled = isEnabled;
        }
    }
    
}
