using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedCorrection : IActorAction {

        [Range(0f, 1f)] public float sideCorrection = 1f;
        [Range(0f, 1f)] public float backCorrection = 1f;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var motion = context.GetComponent<CharacterMotionPipeline>();
            motion.SpeedCorrectionBack = backCorrection;
            motion.SpeedCorrectionSide = sideCorrection;

            return default;
        }
    }
    
}
