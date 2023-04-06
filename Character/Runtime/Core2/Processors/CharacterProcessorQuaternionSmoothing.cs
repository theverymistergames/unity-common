using System;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    [Serializable]
    public sealed class CharacterProcessorQuaternionSmoothing : ICharacterProcessorQuaternion {

        public float smoothFactor = 1f;

        private Quaternion _currentInput;
        private Quaternion _targetInput;

        public Quaternion Process(Quaternion input, float dt) {
            _targetInput = input;
            _currentInput = Quaternion.Slerp(_currentInput, _targetInput, dt * smoothFactor);
            
            return _currentInput;
        }
    }

}
