using System;
using MisterGames.Character.Access;
using MisterGames.Character.Actions;
using MisterGames.Character.Processors;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedMultiplier : ICharacterAction {

        [Min(0f)] public float speed;

        public void Apply(object source, ICharacterAccess characterAccess) {
            var multiplier = characterAccess.MotionPipeline.GetProcessor<CharacterProcessorVector2Multiplier>();
            if (multiplier == null) return;

            multiplier.multiplier = speed;
        }
    }
    
}
