using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Character.Processors;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetSpeedMultiplier : IActorAction {

        [Min(0f)] public float speed;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var multiplier = context.GetComponent<CharacterMotionPipeline>().GetProcessor<CharacterProcessorVector2Multiplier>();

            multiplier.multiplier = speed;
            return default;
        }
    }
    
}
