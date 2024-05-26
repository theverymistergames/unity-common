using MisterGames.Common;
using UnityEngine;

namespace MisterGames.Character.View {
    
    public sealed class CharacterHeadJoint {

        private Transform _target;
        private Vector3 _targetPoint;
        private Quaternion _targetRotation;
        private float _smoothing;
        private AttachMode _attachMode;
        
        private enum AttachMode {
            Free,
            Point,
            Transform,
            TransformWithoutRotation
        }
        
        public void Attach(Transform target, Vector3 point, float smoothing, bool rotate) {
            Debug.Log($"CharacterHeadJoint.Attach: {target}, smoothing {smoothing}");
            _target = target;
            _targetPoint = point - target.position;
            _targetRotation = target.rotation;
            _smoothing = smoothing;
            _attachMode = rotate ? AttachMode.Transform : AttachMode.TransformWithoutRotation;
        }

        public void Attach(Vector3 point, float smoothing) {
            _targetPoint = point;
            _smoothing = smoothing;
            _attachMode = AttachMode.Point;
        }

        public void Detach() {
            Debug.Log($"CharacterHeadJoint.Detach: ");
            _target = null;
            _attachMode = AttachMode.Free;
        }

        public void Update(ref Vector3 position, float dt) {
            var targetPoint = _attachMode switch {
                AttachMode.Point => _targetPoint,
                AttachMode.Transform => _target.position + _target.rotation * Quaternion.Inverse(_targetRotation) * _targetPoint,
                AttachMode.TransformWithoutRotation => _target.position + _targetPoint,
                _ => position,
            };
            
            DebugExt.DrawSphere(targetPoint, 0.01f, Color.red);

            position = _smoothing > 0f ? Vector3.Lerp(position, targetPoint, dt * _smoothing) : targetPoint;
        }
    }
    
}