using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Common;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterLandingDetector : MonoBehaviour, IActorComponent, IUpdate {
        
        [SerializeField] [Min(0f)] private float _maxVerticalSpeed = 10f;
        [SerializeField] [Min(0f)] private float _skipNotGroundedDuration = 0.1f;
        [SerializeField] [Min(0f)] private float _forceNotGroundedAfterJumpDuration = 0.25f;
        [SerializeField] private float _landingPointOffset;
        [SerializeField] [Min(1)] private int _speedBufferSize = 4;

        public delegate void LandingCallback(Vector3 point, float relativeSpeed);
        public event LandingCallback OnLanded = delegate {  };

        public float MaxVerticalSpeed => _maxVerticalSpeed;
        public float RelativeVerticalSpeed { get; private set; }
        public bool IsGrounded { get; private set; }

        private IActor _actor;
        private CharacterGroundDetector _groundDetector;
        private CharacterMotionPipeline _motionPipeline;
        private CharacterJumpPipeline _jumpPipeline;
        private CharacterGravity _gravity;

        private float[] _speedBuffer;
        private int _bufferPointer;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
            _motionPipeline = actor.GetComponent<CharacterMotionPipeline>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _jumpPipeline = actor.GetComponent<CharacterJumpPipeline>();
            _gravity = actor.GetComponent<CharacterGravity>();
            
            _speedBuffer = new float[_speedBufferSize];
        }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            RelativeVerticalSpeed = GetRelativeVerticalSpeed(_motionPipeline.Velocity); 
            WriteSpeedIntoBuffer(RelativeVerticalSpeed);
            
            float time = Time.time;
            bool isGrounded = time > _jumpPipeline.JumpImpulseTime + _forceNotGroundedAfterJumpDuration &&
                              (_groundDetector.HasContact || time - _groundDetector.LastGroundedTime < _skipNotGroundedDuration);

            if (IsGrounded == isGrounded) return;
            
            IsGrounded = isGrounded;
            if (!isGrounded) return;
            
            var point = _actor.Transform.TransformPoint(Vector3.up * _landingPointOffset);
            float relativeSpeed = GetMinimumSpeedFromBuffer();
            
            ClearSpeedBuffer();
            
            OnLanded.Invoke(point, relativeSpeed);
        }

        private void WriteSpeedIntoBuffer(float verticalSpeed) {
            _speedBuffer[_bufferPointer++ % _speedBufferSize] = verticalSpeed;
        }

        private void ClearSpeedBuffer() {
            _bufferPointer = 0;
        }

        private float GetMinimumSpeedFromBuffer() {
            int count = Mathf.Min(_bufferPointer, _speedBufferSize);
            float min = float.MaxValue;
            
            for (int i = 0; i < count; i++) {
                if (_speedBuffer[i] < min) min = _speedBuffer[i];
            }
            
            return min < float.MaxValue ? min : 0f;
        }
        
        private float GetRelativeVerticalSpeed(Vector3 velocity) {
            return _maxVerticalSpeed > 0f 
                ? -VectorUtils.SignedMagnitudeOfProject(velocity, _gravity.GravityDir) / _maxVerticalSpeed 
                : 0f;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            var t = _actor == null ? transform : _actor.Transform;
            DebugExt.DrawSphere(t.TransformPoint(Vector3.up * _landingPointOffset), 0.03f, Color.cyan);
            
            if (Application.isPlaying) {
                Handles.Label(transform.TransformPoint(Vector3.up), $"Rel vertical speed: {RelativeVerticalSpeed:0.000}");
            }
        }
#endif
    }
}