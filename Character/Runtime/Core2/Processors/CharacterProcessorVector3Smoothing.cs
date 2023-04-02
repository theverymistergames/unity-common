using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    [Serializable]
    public sealed class CharacterProcessorVector3Smoothing : ICharacterProcessorVector3 {

        [SerializeField] private float _smoothFactor = 20f;

        public float SmoothFactor { get => _smoothFactor; set => _smoothFactor = value; }

        private Vector3 _currentInput;
        private Vector3 _targetInput;

        public Vector3 Process(Vector3 input, float dt) {
            _targetInput = input;
            _currentInput = Vector2.Lerp(_currentInput, _targetInput, dt * SmoothFactor);
            
            return _currentInput;
        }

        public void SetValueImmediate(Vector3 value) {
            _targetInput = value;
            _currentInput = _targetInput;
        }
    }

}
