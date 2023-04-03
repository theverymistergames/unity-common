using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    [Serializable]
    public sealed class CharacterProcessorVector2Multiplier : ICharacterProcessorVector2 {

        public float multiplier = 1f;

        public Vector2 Process(Vector2 input, float dt) {
            return input * multiplier;
        }
    }

}
