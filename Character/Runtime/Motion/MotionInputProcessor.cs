using System;
using MisterGames.Character.Collisions;
using MisterGames.Character.Configs;
using MisterGames.Character.Input;
using MisterGames.Common.Maths;
using MisterGames.Dbg.Draw;
using MisterGames.Fsm.Core;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public class MotionInputProcessor : MonoBehaviour, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        
        [Header("Controls")]
        [SerializeField] private CharacterInput _input;
        [SerializeField] private CharacterAdapter _adapter;
        
        [Header("States")]
        [SerializeField] private StateMachineRunner _motionFsm;
        
        [Header("Collision")]
        [SerializeField] private CharacterGroundDetector _groundDetector;
        
        [Header("Settings")]
        [SerializeField] private MotionSettings _motionSettings;

        public event Action OnStartMoving = delegate {  };
        public event Action OnStopMoving = delegate {  };
        
        public bool IsMoving { get; private set; }
        public float TargetSpeed => _speedProcessor.Speed;
        
        private readonly SpeedProcessor _speedProcessor = new SpeedProcessor();
        
        private Vector2 _inputDirection;
        private Vector3 _targetDirection;
        private Vector3 _currentDirection;
        private Vector3 _motion;

        private bool _isGrounded;

        private void OnEnable() {
            _input.Move += HandleMoveInput;
            _motionFsm.OnEnterState += HandleMotionStateChanged;
            _timeDomain.Source.Subscribe(this);
        }

        private void OnDisable() {
            _input.Move -= HandleMoveInput;
            _motionFsm.OnEnterState -= HandleMotionStateChanged;
            _timeDomain.Source.Unsubscribe(this);
        }

        private void Start() {
            HandleMotionStateChanged(_motionFsm.Instance.CurrentState);
        }
        
        void IUpdate.OnUpdate(float dt) {
            _targetDirection = GetTargetDirection();

            bool wasGrounded = _isGrounded;
            _isGrounded = _groundDetector.CollisionInfo.hasContact;
            
            _currentDirection = wasGrounded && !_isGrounded 
                ? _targetDirection 
                : GetSmoothedDirection(_targetDirection, dt);
            
            _motion = _currentDirection.RotateFromTo(Vector3.up, _groundDetector.CollisionInfo.lastNormal);
            _adapter.Move(_motion);
        }
        
        private void HandleMotionStateChanged(FsmState state) {
            if (state.data is MotionStateData data) {
                _speedProcessor.SetMotionData(data);
            }
            CheckIsMovingChanged();
        }
        
        private void HandleMoveInput(Vector2 input) {
            _inputDirection = input;
            _speedProcessor.SetInputDirection(input);
        }
        
        private void CheckIsMovingChanged() {
            bool wasMoving = IsMoving;
            IsMoving = _speedProcessor.Speed > 0f;
            
            if (!wasMoving && IsMoving) {
                OnStartMoving.Invoke();
                return;
            }
            if (wasMoving && !IsMoving) {
                OnStopMoving.Invoke();
            }
        }
        
        private Vector3 GetTargetDirection() {
            return _speedProcessor.Speed * GetDirectionToLocalSpace(_inputDirection);
        }
        
        private Vector3 GetSmoothedDirection(Vector3 target, float dt) {
            return Vector3.Lerp(_currentDirection, target, dt * _motionSettings.inputSmoothFactor);
        }

        private Vector3 GetDirectionToLocalSpace(Vector2 direction) {
            var worldSpaceDirection = new Vector3(direction.x, 0f, direction.y);
            return _adapter.BodyRotation * worldSpaceDirection;
        }
        
#if UNITY_EDITOR        
        [Header("Debug")]
        [SerializeField] private bool _debugDrawTargetDirection;
        [SerializeField] private bool _debugDrawCurrentDirection;
        [SerializeField] private bool _debugDrawMotion;
        
        private void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            var pos = transform.position;
            if (_debugDrawTargetDirection) DbgRay.Create().From(pos).Dir(_targetDirection).Arrow(0.3f).Color(Color.green).Draw();
            if (_debugDrawCurrentDirection) DbgRay.Create().From(pos).Dir(_currentDirection).Arrow(0.3f).Color(Color.yellow).Draw();
            if (_debugDrawMotion) DbgRay.Create().From(pos).Dir(_motion).Arrow(0.3f).Color(Color.red).Draw();
        }
#endif
        
    }

}
