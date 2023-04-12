using System;

namespace MisterGames.Character.Core2.Height {

    public interface ICharacterHeightPipeline {

        /// <summary>
        /// Called when character height is being changed once per frame.
        /// The first parameter is the height change progress value, is in range [0f .. 1f].
        /// The seconds parameter is the total duration of the requested height change.
        /// </summary>
        event Action<float, float> OnHeightChanged;

        float Height { get; set; }
        float TargetHeight { get; }

        float Radius { get; set; }
        float TargetRadius { get; }

        /// <summary>
        /// Starts each frame height changes from current height towards target height.
        /// A height change pattern can be passed to customize height and camera path.
        /// By default pattern is null, which means height is being changed linearly.
        /// OnFinish callback returns true if target height is reached.
        /// </summary>
        void ApplyHeightChange(
            float targetHeight,
            float targetRadius,
            float duration,
            bool scaleDuration = true,
            ICharacterHeightChangePattern pattern = null,
            Action onFinish = null
        );

        void SetEnabled(bool isEnabled);
    }
}
