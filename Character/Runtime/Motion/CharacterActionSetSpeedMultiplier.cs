using System;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Character.Processors;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedMultiplier : ICharacterAction {

        [Min(0f)] public float speed;

        public void Apply(object source, ICharacterAccess characterAccess) {
            var multiplier = characterAccess
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorVector2Multiplier>();

            multiplier.multiplier = speed;
        }
    }
    
}
