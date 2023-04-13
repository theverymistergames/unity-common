using System;
using UnityEngine;

namespace MisterGames.Character.Processors {

    [Serializable]
    public sealed class CharacterProcessorVector3Smoothing : ICharacterProcessorVector3 {

        public float smoothFactor = 1f;

        private Vector3 _currentInput;
        private Vector3 _targetInput;

        public Vector3 Process(Vector3 input, float dt) {
            _targetInput = input;
            _currentInput = Vector3.Lerp(_currentInput, _targetInput, dt * smoothFactor);
            
            return _currentInput;
        }
    }

}
