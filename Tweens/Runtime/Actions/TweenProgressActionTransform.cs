using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {

    [Serializable]
    public sealed class TweenProgressActionTransform : ITweenProgressAction {

        [SerializeField] private Transform _transform;
        [SerializeField] private OperationTargetType _type = OperationTargetType.Position;
        [SerializeField] private Vector3 _startValue;
        [SerializeField] private Vector3 _endValue;
        [SerializeField] private bool _useLocal = true;

        public enum OperationTargetType {
            Position,
            Rotation,
            Scale,
        }

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public void OnProgressUpdate(float progress) {
            var value = Vector3.Lerp(_startValue, _endValue, progress);

            switch (_type) {
                case OperationTargetType.Position:
                    if (_useLocal) _transform.localPosition = value;
                    else _transform.position = value;
                    break;

                case OperationTargetType.Rotation:
                    if (_useLocal) _transform.localEulerAngles = value;
                    else _transform.eulerAngles = value;
                    break;

                case OperationTargetType.Scale:
                    _transform.localScale = value;
                    break;

                default:
                    throw new NotImplementedException($"Operation type {_type} is not implemented for {nameof(TweenProgressActionTransform)}");
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

            value = _type switch {
                OperationTargetType.Position => _useLocal ? _transform.localPosition : _transform.position,
                OperationTargetType.Rotation => _useLocal ? _transform.localEulerAngles : _transform.eulerAngles,
                OperationTargetType.Scale => _transform.localScale,
                _ => throw new NotImplementedException($"Operation type {_type} is not implemented for {nameof(TweenProgressActionTransform)}"),
            };

            return true;
        }
#endif
    }
}
