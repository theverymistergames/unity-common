using System;
using UnityEngine;

namespace MisterGames.Character.Core2.Height {

    [Serializable]
    public sealed class CharacterHeightChangePatternLinear : ICharacterHeightChangePattern {

        public static readonly ICharacterHeightChangePattern Instance = new CharacterHeightChangePatternLinear();

        public float MapHeight(float height) => height;
        public Quaternion MapHeadRotationOffset(float height) => Quaternion.identity;
        public Vector3 MapHeadPositionOffset(float height) => Vector3.zero;
    }

}
