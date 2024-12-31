using System;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Collisions;
using MisterGames.Character.Input;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class CharacterJumpPipeline : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private float _force = 1f;
        [SerializeField] private bool _infiniteJumps;

        [Tooltip("Max time from jump impulse to takeoff from ground. " +
                 "When the player is trying to jump mid-air using the coyote time, " +
                 "this value is used to prevent double jumps, " +
                 "by deciding if the player jumped or just fell last time ground contact was lost. " +
                 "If the difference between jump start and ground takeoff is less than this value, " +
                 "then it is considered that the player jumped to take off from ground, and coyote jump is unavailable. " +
                 "Otherwise it is considered the player just fell, so coyote jump can be performed.")]
        [SerializeField] [Min(0f)] private float _jumpTakeoffDuration = 0.1f;
        [SerializeField] [Min(0f)] private float _minGroundedTimeToAllowJump = 0.2f;
        [SerializeField] [Min(0f)] private float _retryFailedJumpDuration = 0.15f;
        [SerializeField] [Min(0f)] private float _coyoteTime = 0.2f;
        [SerializeField] [Min(0f)] private float _jumpImpulseDelay;
        [SerializeField] [Range(0f, 90f)] private float _maxSlopeAngle = 30f;
        
        [EmbeddedInspector]
        [SerializeField] private ActorCondition _jumpCondition;

        public event Action OnJumpRequest = delegate {  };
        public event Action<Vector3> OnJumpImpulse = delegate {  };

        public Vector3 LastJumpImpulse { get; private set; }
        public float Force { get => _force; set => _force = value; }
        public bool IsBlocked { get; set; }

        private IActor _actor;
        private CharacterInputPipeline _input;
        private CharacterMotionPipeline _motion;
        private CharacterExtraGravity _extraGravity;
        private CharacterGroundDetector _groundDetector;
        
        private float _jumpPressTime;
        private float _jumpReleaseTime;
        private float _jumpRequestApplyTime;
        private float _jumpImpulseApplyTime;
        private float _lastTimeGrounded;

        private bool _isJumpRequested;
        private bool _isJumpImpulseRequested;

        public void OnAwake(IActor actor) {
            _actor = actor;
            _input = actor.GetComponent<CharacterInputPipeline>();
            _motion = actor.GetComponent<CharacterMotionPipeline>();
            _extraGravity = actor.GetComponent<CharacterExtraGravity>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
        }

        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
            _input.JumpPressed += HandleJumpPressedInput;
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
            _input.JumpPressed -= HandleJumpPressedInput;
        }

        void IUpdate.OnUpdate(float dt) {
            ApplyJumpRequest();
            
            float time = Time.time;
            
            if (!_input.IsJumpPressed) {
                _jumpReleaseTime = time;
            }
            
            if (_groundDetector.HasContact) {
                _lastTimeGrounded = time;
                return;
            }
            
            ApplyFallForce();
        }

        private void HandleJumpPressedInput() {
            if (!CanRequestJump()) return;
            
            _jumpPressTime = Time.time;
            _isJumpRequested = true;
        }
        
        private void ApplyJumpRequest() {
            float time = Time.time;

            // Jump requested: check if can jump or retry.
            if (_isJumpRequested) {
                if (CanApplyJumpImpulse()) {
                    _jumpRequestApplyTime = time;
                    _isJumpImpulseRequested = true;
                    _isJumpRequested = false;
                    
                    OnJumpRequest.Invoke();
                }

                if (time > _jumpPressTime + _retryFailedJumpDuration) _isJumpRequested = false;
            }

            // Jump impulse delay finished.
            if (_isJumpImpulseRequested && time >= _jumpRequestApplyTime + _jumpImpulseDelay) {
                _isJumpImpulseRequested = false;
                ApplyJumpImpulse();
            }
        }

        private void ApplyJumpImpulse() {
            var gravityDirection = _extraGravity.GravityDir;
            var jumpImpulse = Force * -gravityDirection;
            var velocity = _motion.Velocity;

            if (Vector3.Dot(gravityDirection, velocity) > 0f) {
                // Reset vertical speed to always apply identical jump force,
                // if velocity is directed down along gravity. 
                _motion.Velocity = Vector3.ProjectOnPlane(velocity, gravityDirection);
            }
            else if (Vector3.Dot(gravityDirection, _motion.MotionDirWorld) < 0f) {
                // Remove velocity part created by input force.

                var verticalVelocity = Vector3.Project(velocity, gravityDirection);
                var inputVerticalVelocity = Vector3.Project(_motion.MotionDirWorld * _motion.Speed, gravityDirection);

                verticalVelocity -= verticalVelocity.sqrMagnitude > inputVerticalVelocity.sqrMagnitude 
                    ? inputVerticalVelocity
                    : verticalVelocity;

                _motion.Velocity = verticalVelocity + Vector3.ProjectOnPlane(velocity, gravityDirection);
            }

            if (_motion.SlopeAngle >= _maxSlopeAngle) {
                jumpImpulse = Force * _groundDetector.GetAccurateNormal();
            }
            
            _motion.AddForce(jumpImpulse, ForceMode.VelocityChange);
            
            _jumpImpulseApplyTime = Time.time;
            OnJumpImpulse?.Invoke(jumpImpulse);
        }
        
        private void ApplyFallForce() {
            var gravityDirection = _extraGravity.GravityDir;
            var velocity = _motion.Velocity;
            
            float sqrVerticalSpeed = Mathf.Sign(Vector3.Dot(-gravityDirection, velocity)) * 
                                     Vector3.Project(velocity, gravityDirection).sqrMagnitude;

            _extraGravity.IsFallForceAllowed = sqrVerticalSpeed > 0f && _jumpReleaseTime > _jumpRequestApplyTime + _jumpImpulseDelay;
        }
        
        private bool CanRequestJump() {
            return _infiniteJumps || 
                   !IsBlocked && !_force.IsNearlyZero() && 
                   (_jumpCondition == null || _jumpCondition.IsMatch(_actor));
        }
        
        private bool CanApplyJumpImpulse() {
            float time = Time.time;

            return _infiniteJumps || 
                   !IsBlocked && time >= _jumpRequestApplyTime + _jumpImpulseDelay + _jumpTakeoffDuration && 
                   _jumpRequestApplyTime + _jumpImpulseDelay + _jumpTakeoffDuration + _minGroundedTimeToAllowJump <= _lastTimeGrounded && 
                   (_groundDetector.HasContact || time - _lastTimeGrounded <= _coyoteTime);
        }
    }

}
