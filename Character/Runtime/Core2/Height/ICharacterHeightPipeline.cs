using System;

namespace MisterGames.Character.Core2.Height {

    public interface ICharacterHeightPipeline {

        /// <summary>
        /// Called when character height is being changed once per frame.
        /// The first parameter is the height change progress value, is in range [0f .. 1f].
        /// </summary>
        event Action<float> OnHeightChange;

        float CurrentHeight { get; }
        float TargetHeight { get; }

        void SetHeight(float height);
        void SetHeightRatio(float targetRatio);
        void MoveToHeightRatio(float targetRatio, float speedMultiplier);

        void SetEnabled(bool isEnabled);
    }

}
