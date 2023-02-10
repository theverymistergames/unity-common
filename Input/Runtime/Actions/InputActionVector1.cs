using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Input.Bindings;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.Input.Actions {

    [CreateAssetMenu(fileName = nameof(InputActionVector1), menuName = "MisterGames/Input/Action/" + nameof(InputActionVector1))]
    public sealed class InputActionVector1 : InputAction {
        
        [SerializeField] private float _sensitivity = 1f;
        [SerializeField] private bool _ignoreNewValueIfNotChanged;
        [SerializeField] private bool _ignoreZero;

        [SerializeReference] [SubclassSelector] private IVector1Binding[] _bindings;

        public event Action<float> OnChanged = delegate {  };
        
        private float _value;

        protected override void OnInit() { }

        protected override void OnTerminate() {
            ResetAndNotify();
        }

        protected override void OnActivated() { }

        protected override void OnDeactivated() {
            ResetAndNotify();
        }
        
        protected override void OnUpdate(float dt) {
            float prevValue = _value;
            _value = ReadInput();
            
            if (_ignoreNewValueIfNotChanged && prevValue.IsNearlyEqual(_value)) return;
            if (_ignoreZero && _value.IsNearlyZero()) return;
            
            OnChanged.Invoke(_value);
        }

        private float ReadInput() {
            float result = 0f;
            if (_bindings.Length == 0) return result;

            int count = 0;
            for (int i = 0; i < _bindings.Length; i++) {
                float value = _bindings[i].Value;
                if (value.IsNearlyZero()) continue;
                
                result += value;
                count++;
            }

            return (count == 0 ? result : result / count) * _sensitivity;
        }
        
        private void ResetAndNotify() {
            _value = 0;
            OnChanged.Invoke(_value);
        }
    }

}
