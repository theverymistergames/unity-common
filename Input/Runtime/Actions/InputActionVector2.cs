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
        
        [SerializeReference] [SubclassSelector] private IInputBindingVector2[] _bindings;
        [SerializeField] private NormalizeMode _normalize = NormalizeMode.None;
        [SerializeField] private bool _ignoreNewValueIfNotChanged;
        [SerializeField] private bool _ignoreZero;

        private enum NormalizeMode {
            None,
            Always,
            Clamp,
        }

        public event Action<Vector2> OnChanged = delegate {  };
        
        private Vector2 _vector = Vector2.zero;

        protected override void OnInit() {
            foreach (var binding in _bindings) {
                binding.Init();   
            }
        }

        protected override void OnTerminate() {
            foreach (var binding in _bindings) {
                binding.Terminate();   
            }
            
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
                var value = _bindings[i].GetValue();
                if (value.IsNearlyZero()) continue;
                
                result += value;
                count++;
            }

            return count == 0 ? result : result / count;
        }
        
        private void ResetVectorAndNotify() {
            _vector.x = 0f;
            _vector.y = 0f;
            OnChanged.Invoke(_vector);
        }
    }

}
