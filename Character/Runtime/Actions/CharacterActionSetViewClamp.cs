using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Character.View;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Actions {
    
    [Serializable]
    public sealed class CharacterActionSetViewClamp : ICharacterAction {

        public Optional<ViewAxisClamp> horizontal;
        public Optional<ViewAxisClamp> vertical;

        public UniTask Apply(ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var clamp = characterAccess
                .GetPipeline<ICharacterViewPipeline>()
                .GetProcessor<CharacterProcessorViewClamp>();

            var bodyRot = characterAccess.BodyAdapter.Rotation;
            
            if (horizontal.HasValue) {
                float angleY = -90f + Vector3.SignedAngle(
                    Vector3.forward,
                    bodyRot * Vector3.forward,
                    characterAccess.BodyAdapter.Rotation * Vector3.up
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
