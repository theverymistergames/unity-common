using System;
using UnityEngine;

namespace MisterGames.Character.View {
    
    public sealed class CharacterHeadJoint {

        private Transform _target;
        private Vector3 _targetPoint;
        private Quaternion _targetRotation;
        private float _smoothing;
        private Mode _mode;
        private float _distance;
        
        private enum Mode {
            Free,
            Point,
            Transform,
            TransformWithoutRotation,
            TransformLookaround
        }
        
        public void Attach(Transform target, Vector3 point, AttachMode mode, float smoothing) {
            _target = target;
            _targetPoint = point - target.position;
            _distance = _targetPoint.magnitude;
            _targetRotation = target.rotation;
            _smoothing = smoothing;
            
            _mode = mode switch {
                AttachMode.OffsetOnly => Mode.TransformWithoutRotation,
                AttachMode.RotateWithTarget => Mode.Transform,
                AttachMode.RotateAroundTarget => Mode.TransformLookaround,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        public void Attach(Vector3 point, float smoothing) {
            _targetPoint = point;
            _smoothing = smoothing;
            _mode = Mode.Point;
        }

        public void Detach() {
            _target = null;
            _mode = Mode.Free;
        }

        public void Update(ref Vector3 position, Vector2 orientation, float dt) {
            var targetPoint = _mode switch {
                Mode.Point => _targetPoint,
                Mode.Transform => _target.position + _target.rotation * Quaternion.Inverse(_targetRotation) * _targetPoint,
                Mode.TransformWithoutRotation => _target.position + _targetPoint,
                Mode.TransformLookaround => _target.position + Quaternion.Euler(orientation) * Vector3.back * _distance,
                _ => position,
            };
            
            position = _smoothing > 0f ? Vector3.Lerp(position, targetPoint, dt * _smoothing) : targetPoint;
        }
    }
    
}