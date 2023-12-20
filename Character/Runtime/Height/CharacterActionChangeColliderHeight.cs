using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Height {

    [Serializable]
    public sealed class CharacterActionChangeColliderHeight : ICharacterAction {

        public Optional<float> sourceHeight;
        public Optional<float> targetHeight;
        public Optional<float> targetRadius;

        [Min(0f)] public float metersPerSecond;

        public UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default) {
            var height = characterAccess.GetPipeline<ICharacterHeightPipeline>();

            if (targetRadius.HasValue) height.Radius = targetRadius.Value;

            float currentHeight = height.Height;

            float fromHeight = sourceHeight.GetOrDefault(currentHeight);
            float toHeight = targetHeight.GetOrDefault(currentHeight);

            float duration = metersPerSecond <= 0f ? 0f : Mathf.Abs(toHeight - fromHeight) / metersPerSecond;

            return height.ApplyHeightChange(fromHeight, toHeight, duration, cancellationToken);
        }
    }

}
