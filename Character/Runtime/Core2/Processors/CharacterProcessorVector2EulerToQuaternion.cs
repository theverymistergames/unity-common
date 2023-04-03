using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    [Serializable]
    public sealed class CharacterProcessorVector2EulerToQuaternion : ICharacterProcessorVector2ToQuaternion {

        public Mode mode;

        public enum Mode {
            XYO,
            XOY,
            OXY,
        }

        public Quaternion Process(Vector2 input, float dt) {
            return mode switch {
                Mode.XYO => Quaternion.Euler(input.x, input.y, 0f),
                Mode.XOY => Quaternion.Euler(input.x, 0f, input.y),
                Mode.OXY => Quaternion.Euler(0f, input.x, input.y),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

}
