using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Height {

    public interface ICharacterHeightPipeline : ICharacterPipeline {

        /// <summary>
        /// Called when character height is being changed once per frame.
        /// The first parameter is the height change progress value, is in range [0f .. 1f].
        /// The seconds parameter is the total duration of the requested height change.
        /// </summary>
        event Action<float, float> OnHeightChanged;

        float Height { get; set; }
        float TargetHeight { get; }

        float Radius { get; set; }
        Vector3 CenterOffset { get; }

        /// <summary>
        /// Starts each frame height changes from current height towards target height.
        /// </summary>
        UniTask ApplyHeightChange(
            float sourceHeight,
            float targetHeight,
            float duration,
            CancellationToken cancellationToken = default
        );
    }

}
