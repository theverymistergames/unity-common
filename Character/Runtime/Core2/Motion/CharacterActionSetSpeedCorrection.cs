using System;
using MisterGames.Character.Core2.Modifiers;
using UnityEngine;

namespace MisterGames.Character.Core2.Motion {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedCorrection : ICharacterAction {

        [Range(0f, 1f)] public float sideCorrection = 1f;
        [Range(0f, 1f)] public float backCorrection = 1f;

        public void Apply(object source, ICharacterAccess characterAccess) {
            var correction = characterAccess.MotionPipeline.GetProcessor<CharacterProcessorBackSideSpeedCorrection>();
            if (correction == null) return;

            correction.speedCorrectionBack = backCorrection;
            correction.speedCorrectionSide = sideCorrection;
        }
    }
    
}
