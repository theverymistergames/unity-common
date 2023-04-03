using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    [Serializable]
    public sealed class CharacterBackSideSpeedCorrectionProcessor : ICharacterProcessorVector2 {

        public float speedCorrectionSide = 1f;
        public float speedCorrectionBack = 1f;

        public Vector2 Process(Vector2 input, float dt) {
            return input * CalculateSpeedCorrection(input);
        }

        private float CalculateSpeedCorrection(Vector2 input) {
            // Moving backwards OR backwards + sideways: apply back correction
            if (input.y < 0) return speedCorrectionBack;

            // Moving forwards OR forwards + sideways: no adjustment
            if (input.y > 0) return 1f;

            // Moving sideways only: apply side correction
            return speedCorrectionSide;
        }
    }

}
