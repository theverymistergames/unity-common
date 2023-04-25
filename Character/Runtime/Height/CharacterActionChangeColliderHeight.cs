using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Height {

    [Serializable]
    public sealed class CharacterActionChangeColliderHeight : ICharacterAsyncAction {

        public Optional<float> sourceHeight;
        public Optional<float> targetHeight;
        public Optional<float> targetRadius;

        [Min(0f)] public float metersPerSecond;

        public UniTask ApplyAsync(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            var heightPipeline = characterAccess.GetPipeline<ICharacterHeightPipeline>();

            if (targetRadius.HasValue) heightPipeline.Radius = targetRadius.Value;

            float currentHeight = heightPipeline.Height;

            float fromHeight = sourceHeight.GetOrDefault(currentHeight);
            float toHeight = targetHeight.GetOrDefault(currentHeight);

            float duration = metersPerSecond <= 0f ? 0f : Mathf.Abs(toHeight - fromHeight) / metersPerSecond;

            return heightPipeline.ApplyHeightChange(fromHeight, toHeight, duration, cancellationToken);
        }
    }

}
