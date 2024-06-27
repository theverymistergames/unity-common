using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Character.View {
    
    public sealed class CharacterHeadJoint {

        public event Action<float> OnAttach = delegate { };
        public event Action OnDetach = delegate { };
        
        public float AttachDistance { get; set; }
        public bool IsAttached => _mode != Mode.Free;
        
        private readonly Dictionary<Transform, AttachData> _attachMap = new();
        private readonly Dictionary<Transform, RotationData> _rotationsMap = new();
        private readonly HashSet<Transform> _rotationObjects = new();

        private Transform _target;
        private Vector3 _targetPoint;
        private Vector3 _targetDir;
        private Quaternion _targetRotation;
        private float _smoothing;
        private Mode _mode;

        private enum Mode {
            Free,
            Point,
            Transform,
            TransformWithoutRotation,
            TransformLookaround
        }

        private struct AttachData {
            public Vector3 offset;
            public Quaternion rotation;
            public Quaternion orientation;
            public float smoothing;
        }
        
        private struct RotationData {
            public Vector3 sensitivity;
            public float smoothing;
            public Quaternion rotation;
            public Quaternion targetRotation;
            public Quaternion orientation;
            public RotationPlane plane;
        }
        
        public void Attach(Transform target, Vector3 point, AttachMode mode, float smoothing) {
            var pos = target.position;
            
            _target = target;
            _targetRotation = target.rotation;
            _targetDir = (point - pos).normalized;
            _smoothing = smoothing;
            AttachDistance = (point - pos).magnitude;
            
            _mode = mode switch {
                AttachMode.OffsetOnly => Mode.TransformWithoutRotation,
                AttachMode.RotateWithTarget => Mode.Transform,
                AttachMode.RotateAroundTarget => Mode.TransformLookaround,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
            
            OnAttach.Invoke(AttachDistance);
        }

        public void Attach(Vector3 point, float smoothing) {
            _targetPoint = point;
            _smoothing = smoothing;
            _mode = Mode.Point;
            AttachDistance = 0f;
            
            OnAttach.Invoke(AttachDistance);
        }

        public void Detach() {
            _target = null;
            _mode = Mode.Free;
            
            OnDetach.Invoke();
        }

        public void AttachObject(Transform obj, Vector3 point, Vector3 position, Vector2 orientation, float smoothing = 0f) {
            _attachMap[obj] = new AttachData {
                offset = point - position,
                rotation = obj.rotation,
                orientation = Quaternion.Euler(orientation),
                smoothing = smoothing,
            };
        }

        public void DetachObject(Transform obj) {
            _attachMap.Remove(obj);
        }

        public void RotateObject(
            Transform obj,
            Vector2 orientation,
            Vector2 sensitivity,
            RotationPlane plane = RotationPlane.XY,
            float smoothing = 0f
        ) {
            _rotationObjects.Add(obj);

            var rot = obj.rotation;
            
            _rotationsMap[obj] = new RotationData {
                sensitivity = sensitivity,
                smoothing = smoothing,
                rotation = rot,
                targetRotation = rot,
                orientation = Quaternion.Euler(orientation),
                plane = plane,
            };
        }

        public void StopRotateObject(Transform obj) {
            _rotationsMap.Remove(obj);
            _rotationObjects.Remove(obj);
        }
        
        public void Update(ref Vector3 position, Vector2 orientation, Vector2 delta, float dt) {
            var targetPoint = _mode switch {
                Mode.Point => _targetPoint,
                Mode.Transform => _target.position + _target.rotation * Quaternion.Inverse(_targetRotation) * _targetDir * AttachDistance,
                Mode.TransformWithoutRotation => _target.position + _targetDir * AttachDistance,
                Mode.TransformLookaround => _target.position + Quaternion.Euler(orientation) * Vector3.back * AttachDistance,
                _ => position,
            };
            
            position = _smoothing > 0f ? Vector3.Lerp(position, targetPoint, dt * _smoothing) : targetPoint;
            
            foreach (var (obj, data) in _attachMap) {
                var rot = Quaternion.Euler(orientation) * Quaternion.Inverse(data.orientation);
                var targetPos = position + rot * data.offset;
                
                obj.position = data.smoothing > 0f 
                    ? Vector3.Lerp(obj.position, targetPos, data.smoothing * dt)
                    : targetPos;

                if (_rotationObjects.Contains(obj)) continue;
                
                var targetRot = rot * data.rotation;
                obj.rotation = data.smoothing > 0f 
                    ? Quaternion.Slerp(obj.rotation, targetRot, data.smoothing * dt)
                    : targetRot;
            }
            
            foreach (var obj in _rotationObjects) {
                var data = _rotationsMap[obj];
                var rot = Quaternion.Euler(orientation) * Quaternion.Inverse(data.orientation);

                data.targetRotation = GetRotationDelta(delta, data.sensitivity, data.orientation, data.plane) * data.targetRotation;
                data.rotation = data.smoothing > 0f 
                    ? Quaternion.Slerp(data.rotation, data.targetRotation, dt * data.smoothing)
                    : data.targetRotation;
                
                var targetRot = rot * data.rotation;
                obj.rotation = data.smoothing > 0f 
                    ? Quaternion.Slerp(obj.rotation, targetRot, data.smoothing * dt)
                    : targetRot;

                _rotationsMap[obj] = data;
            }
        }

        private static Quaternion GetRotationDelta(Vector2 delta, Vector2 sensitivity, Quaternion orientation, RotationPlane plane) {
            Vector3 axisX;
            Vector3 axisY;

            switch (plane) {
                case RotationPlane.XY:
                    axisX = new Vector3(1f, 0f, 0f);
                    axisY = new Vector3(0f, 1f, 0f);
                    break;
                
                case RotationPlane.YX:
                    axisX = new Vector3(0f, 1f, 0f);
                    axisY = new Vector3(1f, 0f, 0f);
                    break;
                
                case RotationPlane.XZ:
                    axisX = new Vector3(1f, 0f, 0f);
                    axisY = new Vector3(0f, 0f, 1f);
                    break;
                
                case RotationPlane.ZX:
                    axisX = new Vector3(0f, 0f, 1f);
                    axisY = new Vector3(1f, 0f, 0f);
                    break;
                
                case RotationPlane.YZ:
                    axisX = new Vector3(0f, 1f, 0f);
                    axisY = new Vector3(0f, 0f, 1f);
                    break;
                
                case RotationPlane.ZY:
                    axisX = new Vector3(0f, 0f, 1f);
                    axisY = new Vector3(0f, 1f, 0f);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(plane), plane, null);
            }
            
            return Quaternion.AngleAxis(delta.x * sensitivity.x, orientation * axisX) * 
                   Quaternion.AngleAxis(delta.y * sensitivity.y, orientation * axisY);
        }
    }
    
}