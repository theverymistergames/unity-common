using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterSlopeProcessor : MonoBehaviour, IActorComponent, IMotionProcessor, IUpdate {
        
        [SerializeField] [MinMaxSlider(0f, 90f)] private Vector2 _slopeAngle = new Vector2(25f, 45f);
        
        [Header("Force Correction")]
        [SerializeField] [MinMaxSlider(0f, 180f)] private Vector2 _forceCorrectionTurnAngle = new Vector2(15f, 120f);
        [SerializeField] [MinMaxSlider(0f, 90f)] private Vector2 _forceCorrectionSlopeAngle = new Vector2(3f, 30f);
        [SerializeField] [Range(0f, 1f)] private float _forceCorrectionTurnAngleWeight = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float _forceCorrectionSlopeAngleWeight = 1f;
        
        public float SlopeAngle { get; private set; }
        public Vector2 SlopeAngleLimits => _slopeAngle;
        
        private CharacterMotionPipeline _motionPipeline;
        private CharacterGroundDetector _groundDetector;
        private CharacterGravity _characterGravity;
        
        void IActorComponent.OnAwake(IActor actor)
        {
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _characterGravity = actor.GetComponent<CharacterGravity>();
            _motionPipeline  = actor.GetComponent<CharacterMotionPipeline>();
        }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
            _motionPipeline.AddProcessor(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            _motionPipeline.RemoveProcessor(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var up = _motionPipeline.Up;
            var normal = _groundDetector.CollisionInfo.normal;
            var axis = Vector3.Cross(_motionPipeline.MotionDirWorld, up);
            
            SlopeAngle = axis == default ? 0f : Vector3.SignedAngle(up, normal, axis.normalized);
        }

        bool IMotionProcessor.ProcessOrientation(ref Quaternion orientation, out int priority) {
            priority = 0;
            return false;
        }

        void IMotionProcessor.ProcessInputSpeed(ref float speed, float dt) {
            
        }

        void IMotionProcessor.ProcessInputForce(ref Vector3 inputForce, Vector3 desiredVelocity, float dt) {
            if (!_groundDetector.HasContact || !_characterGravity.HasGravity || _motionPipeline.Input == default) return;

            var velocity = Vector3.ProjectOnPlane(_motionPipeline.Velocity, _motionPipeline.MotionNormal);
            
            ApplyDirCorrection(desiredVelocity, velocity, ref inputForce, dt);
            LimitForceBySlopeAngle(ref inputForce);
        }
        
        private void LimitForceBySlopeAngle(ref Vector3 inputForce) {
            if (SlopeAngle <= _slopeAngle.y) return;
            
            var up = _motionPipeline.Up;
            var normal = _motionPipeline.MotionNormal;
            var slopeUp = Vector3.Cross(Vector3.Cross(normal, up), normal).normalized;
            
            inputForce = Vector3.ProjectOnPlane(inputForce, slopeUp);
        }

        private void ApplyDirCorrection(Vector3 desiredVelocity, Vector3 currentVelocity, ref Vector3 force, float dt) {
            if (desiredVelocity == Vector3.zero) return;

            var nextVelocity = currentVelocity + force * dt;
            var perfectForce = dt > 0f ? (desiredVelocity - currentVelocity) / dt : force;
                
            float turnAngle = Vector3.Angle(desiredVelocity, Vector3.ProjectOnPlane(nextVelocity, _motionPipeline.Up));
            float turnFactor = turnAngle <= _forceCorrectionTurnAngle.y
                ? Mathf.Clamp01((turnAngle - _forceCorrectionTurnAngle.x) / (_forceCorrectionTurnAngle.y - _forceCorrectionTurnAngle.x))
                : 0f;
            
            float slopeFactor = Mathf.Clamp01((Mathf.Abs(SlopeAngle) - _forceCorrectionSlopeAngle.x) / (_forceCorrectionSlopeAngle.y - _forceCorrectionSlopeAngle.x));
            
            float t = Mathf.Max(turnFactor * _forceCorrectionTurnAngleWeight, slopeFactor * _forceCorrectionSlopeAngleWeight);
            force = Vector3.Lerp(force, perfectForce, t);
        }
    }
    
}