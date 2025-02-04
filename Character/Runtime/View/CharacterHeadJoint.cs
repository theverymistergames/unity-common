using System;
using System.Collections.Generic;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.View {
    
    public sealed class CharacterHeadJoint {

        public event Action<float> OnAttach = delegate { };
        public event Action OnDetach = delegate { };
        
        public float AttachDistance { get; set; }
        public bool IsAttached => _mode != Mode.Free;
        
        private readonly Dictionary<Transform, AttachData> _attachedObjectsMap = new();
        private readonly Dictionary<Transform, RotationData> _rotationsMap = new();
        private readonly HashSet<Transform> _rotationObjects = new();

        private Transform _target;
        private Vector3 _targetPoint;
        private Vector3 _targetDir;
        private Quaternion _targetRotation;
        private Mode _mode;
        private float _smoothing;

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
        
        public void AttachTo(Transform target, Vector3 point, AttachMode mode, float smoothing) {
            var targetPos = target.position;
            
            _target = target;
            _targetPoint = targetPos;
            _targetRotation = target.rotation;
            _targetDir = (point - targetPos).normalized;
            AttachDistance = (point - targetPos).magnitude;
            _smoothing = smoothing;

            _mode = mode switch {
                AttachMode.OffsetOnly => Mode.TransformWithoutRotation,
                AttachMode.RotateWithTarget => Mode.Transform,
                AttachMode.RotateAroundTarget => Mode.TransformLookaround,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
            
            OnAttach.Invoke(AttachDistance);
        }

        public void AttachTo(Vector3 point, float smoothing) {
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
            _attachedObjectsMap[obj] = new AttachData {
                offset = point - position,
                rotation = obj.rotation,
                orientation = Quaternion.Euler(orientation),
                smoothing = smoothing,
            };
        }

        public void DetachObject(Transform obj) {
            _attachedObjectsMap.Remove(obj);
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
        
        public void Update(ref Vector3 position, Quaternion rotationOffset, Vector2 orientation, Vector2 delta, float dt) {
            position = GetPosition(position, rotationOffset, orientation, dt);
            
            var orient = rotationOffset * Quaternion.Euler(orientation);
            
            UpdateAttachedObjects(position, orient, dt);
            UpdateRotationObjects(orient, delta, dt);
        }

        private Vector3 GetPosition(Vector3 position, Quaternion rotationOffset, Vector2 orientation, float dt) {
            switch (_mode) {
                case Mode.Point: 
                    return _smoothing > 0f
                        ? position.SmoothExp(_targetPoint, dt * _smoothing)
                        : _targetPoint;

                case Mode.Transform: 
                    _targetPoint = _target.position + _target.rotation * Quaternion.Inverse(_targetRotation) * _targetDir * AttachDistance; 
                    return _smoothing > 0f
                        ? position.SmoothExp(_targetPoint, dt * _smoothing)
                        : _targetPoint;

                case Mode.TransformWithoutRotation: 
                    _targetPoint = _target.position + _targetDir * AttachDistance;
                    return _smoothing > 0f
                        ? position.SmoothExp(_targetPoint, dt * _smoothing)
                        : _targetPoint;

                case Mode.TransformLookaround:
                    _targetPoint = _smoothing > 0f
                        ? _targetPoint.SmoothExp(_target.position, dt * _smoothing)
                        : _target.position;
                    
                    return _targetPoint + rotationOffset * Quaternion.Euler(orientation) * Vector3.back * AttachDistance;

                default: 
                    return position;
            }
        }

        private void UpdateAttachedObjects(Vector3 position, Quaternion orientation, float dt) {
            foreach (var (obj, data) in _attachedObjectsMap) {
                var rot = orientation * Quaternion.Inverse(data.orientation);
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
        }

        private void UpdateRotationObjects(Quaternion orientation, Vector2 delta, float dt) {
            foreach (var obj in _rotationObjects) {
                var data = _rotationsMap[obj];
                var rot = orientation * Quaternion.Inverse(data.orientation);

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