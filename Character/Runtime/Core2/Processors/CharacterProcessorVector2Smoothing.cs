using System;
using UnityEngine;

namespace MisterGames.Character.Core2.Processors {

    [Serializable]
    public sealed class CharacterProcessorVector2Smoothing : ICharacterProcessorVector2 {

        public float smoothFactor = 1f;

        private Vector2 _currentInput;
        private Vector2 _targetInput;

        public Vector2 Process(Vector2 input, float dt) {
            _targetInput = input;
            _currentInput = Vector2.Lerp(_currentInput, _targetInput, dt * smoothFactor);

            return _currentInput;
        }
    }

}
