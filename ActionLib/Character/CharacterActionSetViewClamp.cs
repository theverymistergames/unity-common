using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.ActionLib.Character {
    
    [Serializable]
    public sealed class CharacterActionSetViewClamp : IActorAction {

        public Optional<ViewAxisClamp> horizontal;
        public Optional<ViewAxisClamp> vertical;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var clamp = context.GetComponent<ICharacterViewPipeline>().GetProcessor<CharacterProcessorViewClamp>();

            var bodyRot = context.GetComponent<CharacterBodyAdapter>().Rotation;
            
            if (horizontal.HasValue) {
                float angleY = -90f + Vector3.SignedAngle(
                    Vector3.forward,
                    bodyRot * Vector3.forward,
                    bodyRot * Vector3.up
                );

                clamp.horizontal = horizontal.Value;
                var b = clamp.horizontal.bounds;
                clamp.horizontal.bounds += (b.x + angleY) * Vector2.one;
            }

            if (vertical.HasValue) {
                clamp.vertical = vertical.Value;
            }
            
            return default;
        }
    }
    
}
