using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedCorrection : ICharacterAction {

        [Range(0f, 1f)] public float sideCorrection = 1f;
        [Range(0f, 1f)] public float backCorrection = 1f;

        public UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default) {
            var correction = characterAccess
                .GetPipeline<ICharacterMotionPipeline>()
                .GetProcessor<CharacterProcessorBackSideSpeedCorrection>();

            correction.speedCorrectionBack = backCorrection;
            correction.speedCorrectionSide = sideCorrection;

            return default;
        }
    }
    
}
