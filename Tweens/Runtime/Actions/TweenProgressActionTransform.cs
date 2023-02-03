using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {

    [Serializable]
    public sealed class TweenProgressActionTransform : ITweenProgressAction {

        [SerializeField] private Transform _transform;
        [SerializeField] private OperationType _operation = OperationType.Move;
        [SerializeField] private Vector3 _startValue;
        [SerializeField] private Vector3 _endValue;
        [SerializeField] private bool _useLocal = true;

        public Transform Transform { get => _transform; set => _transform = value; }
        public OperationType Operation { get => _operation ; set => _operation = value; }
        public Vector3 StartValue { get => _startValue; set => _startValue = value; }
        public Vector3 EndValue { get => _endValue; set => _endValue = value; }
        public bool UseLocal { get => _useLocal; set => _useLocal = value; }

        public enum OperationType {
            Move,
            Rotate,
            Scale,
        }

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public void OnProgressUpdate(float progress) {
            var value = Vector3.Lerp(_startValue, _endValue, progress);

            switch (_operation) {
                case OperationType.Move:
                    if (_useLocal) _transform.localPosition = value;
                    else _transform.position = value;
                    break;

                case OperationType.Rotate:
                    if (_useLocal) _transform.localEulerAngles = value;
                    else _transform.eulerAngles = value;
                    break;

                case OperationType.Scale:
                    _transform.localScale = value;
                    break;

                default:
                    throw new NotImplementedException($"Operation type {_operation} is not implemented for {nameof(TweenProgressActionTransform)}");
            }
        }

#if UNITY_EDITOR
        public void WriteCurrentValueAsStartValue() {
            if (TryGetCurrentValue(out var value)) _startValue = value;
        }

        public void WriteCurrentValueAsEndValue() {
            if (TryGetCurrentValue(out var value)) _endValue = value;
        }

        private bool TryGetCurrentValue(out Vector3 value) {
            if (_transform == null) {
                value = default;
                return false;
            }

            value = _operation switch {
                OperationType.Move => _useLocal ? _transform.localPosition : _transform.position,
                OperationType.Rotate => _useLocal ? _transform.localEulerAngles : _transform.eulerAngles,
                OperationType.Scale => _transform.localScale,
                _ => throw new NotImplementedException($"Operation type {_operation} is not implemented for {nameof(TweenProgressActionTransform)}"),
            };

            return true;
        }
#endif
    }
}
