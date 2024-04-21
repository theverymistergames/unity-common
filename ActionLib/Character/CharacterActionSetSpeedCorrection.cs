using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedCorrection : IActorAction {

        [Range(0f, 1f)] public float sideCorrection = 1f;
        [Range(0f, 1f)] public float backCorrection = 1f;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var correction = context.GetComponent<ICharacterMotionPipeline>().GetProcessor<CharacterProcessorBackSideSpeedCorrection>();

            correction.speedCorrectionBack = backCorrection;
            correction.speedCorrectionSide = sideCorrection;

            return default;
        }
    }
    
}
