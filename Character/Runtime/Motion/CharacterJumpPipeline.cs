using System;
using System.Threading;
using MisterGames.Actors;
using MisterGames.Character.Capsule;
using MisterGames.Character.Input;
using MisterGames.Character.Phys;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class CharacterJumpPipeline : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private float _force = 1f;
        [SerializeField] private bool _infiniteJumps;

        [Header("Timings")]
        [Tooltip("Max time from jump impulse to takeoff from ground. " +
                 "When the player is trying to jump mid-air using the coyote time, " +
                 "this value is used to prevent double jumps, " +
                 "by deciding if the player jumped or just fell last time ground contact was lost. " +
                 "If the difference between jump start and ground takeoff is less than this value, " +
                 "then it is considered that the player jumped to take off from ground, and coyote jump is unavailable. " +
                 "Otherwise it is considered the player just fell, so coyote jump can be performed.")]
        [SerializeField] [Min(0f)] private float _jumpTakeoffDuration = 0.1f;
        [SerializeField] [Min(0f)] private float _retryFailedJumpDuration = 0.15f;
        [SerializeField] [Min(0f)] private float _jumpImpulseDelayDefault = 0.05f;
        
        [Header("Conditions")]
        [SerializeField] [Min(0f)] private float _minGroundedTimeToAllowJump = 0.2f;
        [SerializeField] [Min(0f)] private float _coyoteTime = 0.2f;
        [SerializeField] [Range(0f, 90f)] private float _maxSlopeAngle = 30f;
        [SerializeField] private float _minCeilingHeight = 0.3f;

        public event Action OnJumpRequest = delegate {  };
        public event Action<Vector3> OnJumpImpulse = delegate {  };

        public Vector3 LastJumpImpulse { get; private set; }
        public float Force { get => _force; set => _force = value; }
        public float JumpImpulseTime => _jumpImpulseApplyTime;

        private readonly PrioritySet<IJumpOverride> _jumpOverrides = new();
        private readonly BlockSet _blockSet = new();

        private CharacterInputPipeline _input;
        private CharacterMotionPipeline _motion;
        private CharacterGravity _gravity;
        private CharacterGroundDetector _groundDetector;
        private CharacterCeilingDetector _ceilingDetector;
        private CharacterCapsulePipeline _capsulePipeline;
        private CharacterSlopeProcessor _slopeProcessor;
        
        private float _jumpRequestTime;
        private float _jumpReleaseTime;
        private float _jumpRequestApplyTime;
        private float _jumpImpulseApplyTime;
        private float _jumpImpulseDelay;
        private float _lastTimeGrounded;

        private bool _isJumpRequested;
        private bool _isJumpImpulseRequested;
        
        void IActorComponent.OnAwake(IActor actor) {
            _input = actor.GetComponent<CharacterInputPipeline>();
            _motion = actor.GetComponent<CharacterMotionPipeline>();
            _gravity = actor.GetComponent<CharacterGravity>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _ceilingDetector = actor.GetComponent<CharacterCeilingDetector>();
            _capsulePipeline  = actor.GetComponent<CharacterCapsulePipeline>();
            _slopeProcessor = actor.GetComponent<CharacterSlopeProcessor>();
        }

        private void OnDestroy() {
            _blockSet.Clear();
        }

        private void OnEnable() {
            PlayerLoopStage.Update.Subscribe(this);
            
            _input.JumpPressed += HandleJumpPressedInput;
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
            
            _input.JumpPressed -= HandleJumpPressedInput;
        }
        
        public void SetBlock(object source, bool blocked, CancellationToken cancellationToken = default) {
            _blockSet.SetBlock(source, blocked, cancellationToken);
        }
        
        public void RequestJump() {
            float jumpImpulseDelay = _jumpImpulseDelayDefault;
            if (!CanRequestJump(ref jumpImpulseDelay)) return;

            _jumpRequestTime = Time.time;
            _jumpImpulseDelay = jumpImpulseDelay;
            _isJumpRequested = true;
        }
        
        public void StartOverride(IJumpOverride jumpOverride, int priority)
        {
            _jumpOverrides.Set(jumpOverride, priority);
        }

        public void StopOverride(IJumpOverride jumpOverride)
        {
            _jumpOverrides.Remove(jumpOverride);
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
            RequestJump();
        }
        
        private void ApplyJumpRequest() {
            float time = Time.time;

            // Jump requested: check if can jump or retry.
            if (_isJumpRequested) {
                if (CanApplyJumpImpulse(ref _jumpImpulseDelay)) {
                    _jumpRequestApplyTime = time;
                    _isJumpImpulseRequested = true;
                    _isJumpRequested = false;
                    
                    OnJumpRequest.Invoke();
                }

                if (time > _jumpRequestTime + _retryFailedJumpDuration) _isJumpRequested = false;
            }

            // Jump impulse delay finished.
            if (_isJumpImpulseRequested && time >= _jumpRequestApplyTime + _jumpImpulseDelay) {
                _isJumpImpulseRequested = false;
                ApplyJumpImpulse();
            }
        }

        private void ApplyJumpImpulse() {
            var gravityDirection = _gravity.GravityDirection;
            var jumpImpulse = Force * -gravityDirection;
            
            if (_jumpOverrides.TryGetResult(out var jumpOverride) && 
                !jumpOverride.OnJumpImpulseRequested(ref jumpImpulse)) 
            {
                return;
            }
            
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

            if (_slopeProcessor.SlopeAngle >= _maxSlopeAngle) {
                jumpImpulse = Force * _groundDetector.GetAccurateNormal();
            }
            
            _motion.AddForce(jumpImpulse, ForceMode.VelocityChange);
            
            _jumpImpulseApplyTime = Time.time;
            OnJumpImpulse?.Invoke(jumpImpulse);
        }
        
        private void ApplyFallForce() {
            var gravityDirection = _gravity.GravityDirection;
            var velocity = _motion.Velocity;
            
            float sqrVerticalSpeed = Mathf.Sign(Vector3.Dot(-gravityDirection, velocity)) * 
                                     Vector3.Project(velocity, gravityDirection).sqrMagnitude;

            _gravity.IsFallForceAllowed = sqrVerticalSpeed > 0f && _jumpReleaseTime > _jumpRequestApplyTime + _jumpImpulseDelay;
        }
        
        private bool CanRequestJump(ref float jumpImpulseDelay) {
            bool hasOverride = _jumpOverrides.TryGetResult(out var jumpOverride);

            return _infiniteJumps ||
                   hasOverride && jumpOverride.OnJumpRequested(ref jumpImpulseDelay) ||
                   !hasOverride && _blockSet.Count == 0 && !_force.IsNearlyZero();
        }
        
        private bool CanApplyJumpImpulse(ref float impulseDelay) {
            float time = Time.time;
            float lastJumpTakeoffTime = JumpImpulseTime + _jumpTakeoffDuration;
            bool hasOverride = _jumpOverrides.TryGetResult(out var jumpOverride);
            
            return _infiniteJumps || 
                   
                   hasOverride && jumpOverride.OnJumpRequested(ref impulseDelay) ||
                   
                   !hasOverride && _blockSet.Count == 0 &&
                   time >= lastJumpTakeoffTime &&
                   lastJumpTakeoffTime + _minGroundedTimeToAllowJump <= _lastTimeGrounded && 
                   (_groundDetector.HasContact || time - _lastTimeGrounded <= _coyoteTime) && 
                   HasNoCeiling();
        }

        private bool HasNoCeiling() {
            var info = _ceilingDetector.CollisionInfo;
            return !info.hasContact ||
                   (info.point - _capsulePipeline.ColliderTop).sqrMagnitude > _minCeilingHeight * _minCeilingHeight;
        }
    }

}
