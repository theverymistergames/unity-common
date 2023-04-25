using System;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.Actions;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedCorrection : ICharacterAction {

        [Range(0f, 1f)] public float sideCorrection = 1f;
        [Range(0f, 1f)] public float backCorrection = 1f;

        public void Apply(object source, ICharacterAccess characterAccess) {
            var correction = characterAccess
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorBackSideSpeedCorrection>();

            correction.speedCorrectionBack = backCorrection;
            correction.speedCorrectionSide = sideCorrection;
        }
    }
    
}
