using System;
using MisterGames.Actors;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Logic.Transforms {
    
    public sealed class LookAtBehaviour : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private Transform _root;
        [SerializeField] private Transform[] _rotationTargets;
        [SerializeField] private Transform _lookTarget;
        
        [Header("Motion")]
        [SerializeField] private bool _preserveOriginRotationOffset;
        [SerializeField] [Min(0f)] private float _smoothing = 5f;
        [SerializeField] [Range(-180f, 180f)] private float _minAngleHorizontal;
        [SerializeField] [Range(-180f, 180f)] private float _maxAngleHorizontal;
        [SerializeField] [Range(-180f, 180f)] private float _minAngleVertical;
        [SerializeField] [Range(-180f, 180f)] private float _maxAngleVertical;

        [Header("Random")]
        [SerializeField] private IdleMode _idleMode;
        [SerializeField] [MinMaxSlider(0f, 100f)] private Vector2 _changeDirectionTime;
        [SerializeField] [MinMaxSlider(0f, 100f)] private Vector2 _pointDistance;
        [SerializeField] [Range(0f, 1f)] private float _separateRandomDirection;

        public enum IdleMode {
            Forward,
            Random,
        }
        
        private enum LookAtMode {
            None,
            Point,
            Transform,
        }
        
        private Quaternion[] _rotationOffsets;
        private Vector3[] _lookDirs;
        private float[] _nextDirectionChangeTimes;
        private LookAtMode _lookAtMode;
        private Vector3 _targetPoint;

        private void Awake() {
            _rotationOffsets = new Quaternion[_rotationTargets.Length];
            _lookDirs = new Vector3[_rotationTargets.Length];
            _nextDirectionChangeTimes = new float[_rotationTargets.Length];

            var rot = _root.rotation;
            var dir = Vector3.forward * _pointDistance.GetRandomInRange();
            
            for (int i = 0; i < _rotationOffsets.Length; i++) {
                _rotationOffsets[i] = Quaternion.Inverse(rot) * _rotationTargets[i].rotation;
            }
            
            for (int i = 0; i < _lookDirs.Length; i++) {
                _lookDirs[i] = dir;
            }
        }

        private void OnEnable() {
            LookAt(_lookTarget);
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        public void SetIdleMode(IdleMode mode) {
            _idleMode = mode;
        }

        public void LookAt(Transform target) {
            _lookAtMode = target == null ? LookAtMode.None : LookAtMode.Transform;
            _lookTarget = target;
        }

        public void LookAt(Vector3 point) {
            _lookAtMode = LookAtMode.Point;
            _targetPoint = point;
            _lookTarget = null;
        }

        public void StopLookAt() {
            _lookAtMode = LookAtMode.None;
            _lookTarget = null;
        }
        
        void IUpdate.OnUpdate(float dt) {
            switch (_idleMode) {
                case IdleMode.Forward:
                    ProcessForwardDirection();
                    break;

                case IdleMode.Random:
                    ProcessRandomDirection();
                    break;
            
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            ProcessRotation(dt);
        }

        private void ProcessForwardDirection() {
            for (int i = 0; i < _lookDirs.Length; i++) {
                _lookDirs[i] = Vector3.forward * _pointDistance.y;
            }
        }
        
        private void ProcessRandomDirection() {
            float time = Time.time;
            bool changedOne = false;
            
            for (int i = 0; i < _nextDirectionChangeTimes.Length; i++) {
                if (time < _nextDirectionChangeTimes[i]) continue;
                
                _nextDirectionChangeTimes[i] = time + _changeDirectionTime.GetRandomInRange();
                _lookDirs[i] = GetRandomDir();
                
                changedOne = true;
            }

            if (!changedOne || Random.value < _separateRandomDirection) return;
            
            float nextTimeTogether = time + _changeDirectionTime.GetRandomInRange();
            var nextDir = GetRandomDir();
                
            for (int i = 0; i < _nextDirectionChangeTimes.Length; i++) { 
                _nextDirectionChangeTimes[i] = nextTimeTogether;
                _lookDirs[i] = nextDir;
            }
        }
        
        private void ProcessRotation(float dt) {
            _root.GetPositionAndRotation(out var pos, out var rot);
            
            var lookAtPoint = _lookAtMode switch {
                LookAtMode.None => default,
                LookAtMode.Point => ApplyLimits(_targetPoint),
                LookAtMode.Transform => ApplyLimits(_lookTarget.position),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            for (int i = 0; i < _rotationTargets.Length; i++) {
                var rotationTarget = _rotationTargets[i];
                
                var point = _lookAtMode switch {
                    LookAtMode.None => pos + rot * _lookDirs[i],
                    LookAtMode.Point => lookAtPoint,
                    LookAtMode.Transform => lookAtPoint,
                    _ => throw new ArgumentOutOfRangeException()
                };

#if UNITY_EDITOR
                if (_showDebugInfo) DebugExt.DrawLine(rotationTarget.position, point, Color.magenta);
                if (_showDebugInfo) DebugExt.DrawSphere(point, 0.03f, Color.magenta);
#endif
                
                var targetRot = Quaternion.LookRotation(point - rotationTarget.position, rot * Vector3.up) * 
                                (_preserveOriginRotationOffset ? _rotationOffsets[i] : Quaternion.identity);
                
                rotationTarget.rotation = rotationTarget.rotation.SlerpNonZero(targetRot, _smoothing, dt);
            }
        }

        private Vector3 ApplyLimits(Vector3 point) {
            _root.GetPositionAndRotation(out var pos, out var rot);
            
            var dir = point - pos;
            var forward = rot * Vector3.forward;
            
            var rotOffset = Quaternion.Inverse(rot) * Quaternion.LookRotation(dir, rot * Vector3.up);
            var angles = rotOffset.ToEulerAngles180();
            
            angles.x = Mathf.Clamp(angles.x, _minAngleVertical, _maxAngleVertical);
            angles.y = Mathf.Clamp(angles.y, _minAngleHorizontal, _maxAngleHorizontal);
            
            return pos + Quaternion.FromToRotation(dir, Quaternion.Euler(-angles.x, angles.y, 0f) * forward) * dir;
        }

        private Vector3 GetRandomDir() {
            var rot = Quaternion.Euler(
                Random.Range(_minAngleVertical, _maxAngleVertical),
                Random.Range(_minAngleHorizontal, _maxAngleHorizontal),
                0f
            );
            
            return rot * (_pointDistance.GetRandomInRange() * Vector3.forward);
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void OnDrawGizmos() {
            if (!_showDebugInfo || _root == null) return;

            var pos = _root.position;
            var rot = _root.rotation;
            
            var dir00 = Quaternion.Euler(_minAngleVertical, _minAngleHorizontal, 0f) * Vector3.forward;
            var dir01 = Quaternion.Euler(_minAngleVertical, _maxAngleHorizontal, 0f) * Vector3.forward;
            var dir10 = Quaternion.Euler(_maxAngleVertical, _minAngleHorizontal, 0f) * Vector3.forward;
            var dir11 = Quaternion.Euler(_maxAngleVertical, _maxAngleHorizontal, 0f) * Vector3.forward;

            var p00Close = pos + rot * dir00 * _pointDistance.x;
            var p00Far = pos + rot * dir00 * _pointDistance.y;
            
            var p01Close = pos + rot * dir01 * _pointDistance.x;
            var p01Far = pos + rot * dir01 * _pointDistance.y;
            
            var p10Close = pos + rot * dir10 * _pointDistance.x;
            var p10Far = pos + rot * dir10 * _pointDistance.y;
            
            var p11Close = pos + rot * dir11 * _pointDistance.x;
            var p11Far = pos + rot * dir11 * _pointDistance.y;
            
            DebugExt.DrawLine(p00Close, p00Far, Color.cyan, gizmo: true);
            DebugExt.DrawLine(p01Close, p01Far, Color.cyan, gizmo: true);
            DebugExt.DrawLine(p10Close, p10Far, Color.cyan, gizmo: true);
            DebugExt.DrawLine(p11Close, p11Far, Color.cyan, gizmo: true);
            
            DebugExt.DrawLine(p00Close, p01Close, Color.cyan, gizmo: true);
            DebugExt.DrawLine(p00Close, p10Close, Color.cyan, gizmo: true);
            DebugExt.DrawLine(p11Close, p01Close, Color.cyan, gizmo: true);
            DebugExt.DrawLine(p11Close, p10Close, Color.cyan, gizmo: true);
            
            DebugExt.DrawLine(p00Far, p01Far, Color.cyan, gizmo: true);
            DebugExt.DrawLine(p00Far, p10Far, Color.cyan, gizmo: true);
            DebugExt.DrawLine(p11Far, p01Far, Color.cyan, gizmo: true);
            DebugExt.DrawLine(p11Far, p10Far, Color.cyan, gizmo: true);
        }
#endif
    }
    
}