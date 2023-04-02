using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    [Serializable]
    public sealed class CharacterProcessorSpeed : ICharacterProcessorVector2 {

        [SerializeField] private float _speed = 5f;

        public float Speed { get => _speed; set => _speed = value; }

        public Vector2 Process(Vector2 input, float dt) {
            return input * Speed;
        }
    }

}
