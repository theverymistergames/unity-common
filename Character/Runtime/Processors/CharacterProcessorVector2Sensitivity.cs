using System;
using UnityEngine;

namespace MisterGames.Character.Processors {

    [Serializable]
    public sealed class CharacterProcessorVector2Sensitivity : ICharacterProcessorVector2 {

        public Vector2 sensitivity = new Vector2(1f, 1f);

        public Vector2 Process(Vector2 input, float dt) {
            return new Vector2(input.x * sensitivity.x, input.y * sensitivity.y);
        }
    }

}
