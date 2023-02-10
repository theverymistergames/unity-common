using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Lists;
using MisterGames.Common.Maths;
using MisterGames.Input.Bindings;
using MisterGames.Input.Core;
using UnityEngine;

namespace MisterGames.Input.Actions {

    [CreateAssetMenu(fileName = nameof(InputActionVector2), menuName = "MisterGames/Input/Action/" + nameof(InputActionVector2))]
    public sealed class InputActionVector2 : InputAction {

        [SerializeField] private NormalizeMode _normalize;
        [SerializeField] private float _sensitivity = 1f;
        [SerializeField] private bool _ignoreNewValueIfNotChanged;
        [SerializeField] private bool _ignoreZero;

        [SerializeReference] [SubclassSelector] private IVector2Binding[] _bindings;

        private enum NormalizeMode {
            None,
            Always,
            Clamp,
        }

        public event Action<Vector2> OnChanged = delegate {  };
        
        private Vector2 _vector = Vector2.zero;

        protected override void OnInit() { }

        protected override void OnTerminate() {
            ResetVectorAndNotify();
        }

        protected override void OnActivated() { }

        protected override void OnDeactivated() {
            ResetVectorAndNotify();
        }
        
        protected override void OnUpdate(float dt) {
            var prevVector = _vector;
            _vector = ReadInput();

            switch (_normalize) {
                case NormalizeMode.Always:
                    _vector.Normalize();
                    break;
                
                case NormalizeMode.Clamp when _vector.sqrMagnitude > 1f:
                    _vector.Normalize();
                    break;
            }
            
            if (_ignoreNewValueIfNotChanged && prevVector.IsEqual(_vector)) return;
            if (_ignoreZero && _vector.IsNearlyZero()) return;
            
            OnChanged.Invoke(_vector);
        }

        private Vector2 ReadInput() {
            var result = Vector2.zero;
            if (_bindings.Length == 0) return result;

            int count = 0;
            for (int i = 0; i < _bindings.Length; i++) {
                var value = _bindings[i].Value;
                if (value.IsNearlyZero()) continue;
                
                result += value;
                count++;
            }

            return (count == 0 ? result : result / count) * _sensitivity;
        }
        
        private void ResetVectorAndNotify() {
            _vector.x = 0f;
            _vector.y = 0f;
            OnChanged.Invoke(_vector);
        }
    }

}
