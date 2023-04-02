using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    [Serializable]
    public sealed class CharacterProcessorVector2Smoothing : ICharacterProcessorVector2 {

        [SerializeField] private float _smoothFactor = 20f;

        public float SmoothFactor { get => _smoothFactor; set => _smoothFactor = value; }

        private Vector2 _currentInput;
        private Vector2 _targetInput;

        public Vector2 Process(Vector2 input, float dt) {
            _targetInput = input;
            _currentInput = Vector2.Lerp(_currentInput, _targetInput, dt * SmoothFactor);

            return _currentInput;
        }

        public void SetValueImmediate(Vector2 value) {
            _targetInput = value;
            _currentInput = _targetInput;
        }
    }

}
