using System;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Interact.Objects {
    
    [RequireComponent(typeof(InteractiveGrab))]
    public sealed class InteractiveDrawer : MonoBehaviour, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [Header("Positions")]
        [SerializeField] private Vector3 _positionClosed;
        [SerializeField] private Vector3 _positionOpened;

        [SerializeField] private InteractiveDrawerConfig _config;
        
        public event Action<float, float> OnMove = delegate {  };

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);
        private InteractiveGrab _grab;
        private Transform _transform;
        
        private Vector3 _grabDelta;
        private Vector3 _targetPosition;
        
        private Vector3 _openCloseDirection;
        private float _openCloseDistance;
        private float _openCloseDistanceSqr;
        private float _minSpeedSqr;
        
        private Vector3 _intertiaVector;
        private float _intertiaMagnitude;
        private float _intertiaDirection;

        private bool _isGrabbing;
        private bool _isOpenCloseInvalid;

        private Vector3 _startSnapPosition;
        private Vector3 _targetSnapPosition;
        private float _snapDistance;
        private bool _isSnapping;

        private void Reset() {
            var t = transform;
            _positionClosed = t.position;
            _positionOpened = _positionClosed + t.forward;
        }

        private void Awake() {
            _grab = GetComponent<InteractiveGrab>();
            
            _transform = transform;
            _targetPosition = _transform.position;

            _openCloseDirection = _positionOpened - _positionClosed;
            _openCloseDistance = _openCloseDirection.magnitude;
            _openCloseDistanceSqr = _openCloseDistance * _openCloseDistance;

            _isOpenCloseInvalid = _openCloseDistance.IsNearlyZero();

            _minSpeedSqr = _config.minSpeed * _config.minSpeed;
        }

        private void OnEnable() {
            _grab.OnGrab += OnGrab;
            _grab.OnStartGrab += OnStartGrab;
            _grab.OnStopGrab += OnStopGrab;
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            _grab.OnGrab -= OnGrab;
            _grab.OnStartGrab -= OnStartGrab;
            _grab.OnStopGrab -= OnStopGrab;
            _timeSource.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            if (_isOpenCloseInvalid || _isGrabbing) return;

            var prevPosition = _transform.position;
            ConsumeIntertia(dt);
            if (_isSnapping) ApplySnap(dt);

            ApplyPosition();
            ProcessEvents(prevPosition, _targetPosition, dt);
        }

        private void ApplyPosition() {
            _transform.position = _targetPosition;
        }

        private void OnGrab(Vector3 from, Vector3 to) {
            var prevPosition = _transform.position;
            ConsumeGrab(to - from);
            ApplyPosition();
            ProcessEvents(prevPosition, _targetPosition, _timeSource.DeltaTime);
        }

        private void OnStartGrab() {
            _isGrabbing = true;
            _isSnapping = false;
        }

        private void OnStopGrab() {
            _isGrabbing = false;

            _intertiaMagnitude = Math.Min(_config.maxSpeed, _intertiaVector.magnitude / _timeSource.DeltaTime);

            float prevTargetToClosedSqrMag = (_targetPosition - _intertiaVector - _positionClosed).sqrMagnitude;
            float targetToClosedSqrMag = (_targetPosition - _positionClosed).sqrMagnitude;

            _intertiaDirection = prevTargetToClosedSqrMag.IsNearlyEqual(targetToClosedSqrMag) 
                ? 0f
                : targetToClosedSqrMag < prevTargetToClosedSqrMag ? -1f : 1f;
        }
        
        private void ProcessEvents(Vector3 prevPosition, Vector3 position, float dt) {
            var positionDiff = position - prevPosition;
            var velocity = positionDiff / dt;
            
            if (velocity.sqrMagnitude < _minSpeedSqr) return;

            float process = (position - _positionClosed).magnitude / _openCloseDistance;
            float speed = velocity.magnitude;
            
            OnMove.Invoke(process, speed / _config.maxSpeed);
        }

        private void ConsumeGrab(Vector3 delta) {
            var projection = Vector3.Project(delta, _openCloseDirection);
            _targetPosition += projection;

            if ((_targetPosition - _positionClosed).sqrMagnitude > _openCloseDistanceSqr) {
                _targetPosition = _positionOpened;
            }
            else if ((_targetPosition - _positionOpened).sqrMagnitude > _openCloseDistanceSqr) {
                _targetPosition = _positionClosed;
            }

            _intertiaVector = _targetPosition - _transform.position;
        }

        private void ConsumeIntertia(float dt) {
            if (_intertiaMagnitude <= 0f) return;
            
            var currentPosition = _transform.position;
            float closedToCurrentDistance = (_positionClosed - currentPosition).magnitude;
            
            float currentProcess = closedToCurrentDistance / _openCloseDistance;
            float processDiff = _intertiaDirection * _intertiaMagnitude / _openCloseDistance * dt;
            float targetProcess = Mathf.Clamp01(currentProcess + processDiff);
            
            _targetPosition = Vector3.Lerp(_positionClosed, _positionOpened, targetProcess);
            _intertiaMagnitude -= dt * _config.friction;

            if (_intertiaMagnitude <= _config.snapIfSpeedBelow) {
                if (targetProcess <= _config.snapToClosedAtProcess) {
                    _intertiaMagnitude = 0f;
                    StartSnap(_positionClosed);
                    return;
                }
                if (targetProcess >= _config.snapToOpenedAtProcess) {
                    _intertiaMagnitude = 0f;
                    StartSnap(_positionOpened);
                    return;
                }
            }
            
            if (_intertiaMagnitude <= _config.minSpeed) {
                _intertiaMagnitude = 0f;
                return;
            }

            if (0f < targetProcess && targetProcess < 1f) return;
            
            _intertiaDirection *= -1f;
            _intertiaMagnitude *= _config.rebound;
        }

        private void StartSnap(Vector3 targetSnapPosition) {
            _isSnapping = true;
            _startSnapPosition = _targetPosition;
            _targetSnapPosition = targetSnapPosition;
            _snapDistance = (_targetSnapPosition - _startSnapPosition).magnitude;
        }

        private void ApplySnap(float dt) {
            float currentProcess = (_targetPosition - _startSnapPosition).magnitude / _snapDistance;
            float targetProcess = Mathf.Clamp01(currentProcess + dt * _config.snapSpeed / _snapDistance);

            _targetPosition = Vector3.Lerp(_startSnapPosition, _targetSnapPosition, targetProcess);

            if (targetProcess >= 1f) _isSnapping = false;
        }

        public void SaveCurrentPositionAsOpened() {
            _positionOpened = transform.position;
        }
        
        public void SaveCurrentPositionAsClosed() {
            _positionClosed = transform.position;
        }

        public void SetCurrentPositionOpened() {
            transform.position = _positionOpened;
        }
        
        public void SetCurrentPositionClosed() {
            transform.position = _positionClosed;
        }
        
    }

}
