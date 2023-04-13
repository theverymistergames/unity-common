using System;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Processors {

    [Serializable]
    public sealed class CharacterProcessorVector2Clamp : ICharacterProcessorVector2 {

        public ClampMode xMode = ClampMode.None;
        public ClampMode yMode = ClampMode.None;

        public Vector2 lowerBounds;
        public Vector2 upperBounds;

        public Vector2 Process(Vector2 input, float dt) {
            return input.Clamp(xMode, yMode, lowerBounds, upperBounds);
        }
    }

}
