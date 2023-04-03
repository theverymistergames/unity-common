using System;
using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    [Serializable]
    public sealed class CharacterProcessorMass : ICharacterProcessorVector3, ICharacterProcessorInitializable {

        public float airInertialFactor = 1f;
        public float groundInertialFactor = 1f;
        public float gravityForce = -9.8f;

        [SerializeField] private bool _isGravityEnabled = true;

        public bool isGravityEnabled {
            get => isGravityEnabled;
            set {
                _isGravityEnabled = value;
                if (!_isGravityEnabled) _inertialComp.y = 0f;
            }
        }

        private ICharacterPipeline _motionPipeline;
        private ICollisionDetector _groundDetector;
        private ICollisionDetector _ceilingDetector;
        private ICollisionDetector _hitDetector;

        private Vector3 _inertialComp;

        private Vector3 _velocity;
        private Vector3 _lastVelocity;
        private Vector3 _lastInput;

        public void Initialize(ICharacterAccess characterAccess) {
            _motionPipeline = characterAccess.MotionPipeline;

            _hitDetector = characterAccess.HitDetector;
            _ceilingDetector = characterAccess.CeilingDetector;
            _groundDetector = characterAccess.GroundDetector;

            _groundDetector.OnLostContact -= HandleFell;
            _groundDetector.OnLostContact += HandleFell;

            _hitDetector.OnContact -= HandleHit;
            _hitDetector.OnContact += HandleHit;
        }

        public void DeInitialize() {
            _groundDetector.OnLostContact -= HandleFell;
            _hitDetector.OnContact -= HandleHit;
        }

        public void ApplyImpulse(Vector3 impulse) {
            _inertialComp += impulse;
        }

        public void ApplyVelocityChange(Vector3 impulse) {
            _inertialComp = impulse;
        }

        public Vector3 Process(Vector3 input, float dt) {
            _lastInput = input;

            _hitDetector.FetchResults();
            _ceilingDetector.FetchResults();
            _groundDetector.FetchResults();

            UpdateInertia(dt);

            _lastVelocity = _inertialComp + _lastInput;
            return _lastVelocity;
        }

        private void HandleFell() {
            _inertialComp += _lastInput;
            _motionPipeline.GetProcessor<CharacterProcessorVector2Smoothing>()?.SetValueImmediate(Vector2.zero);
        }

        private void HandleHit() {
            float y = _inertialComp.y;
            var xz = _inertialComp.WithY(0f);

            xz = Vector3.ProjectOnPlane(xz, _hitDetector.CollisionInfo.lastNormal);

            _inertialComp = xz.WithY(y);
        }

        private void UpdateInertia(float dt) {
            var groundInfo = _groundDetector.CollisionInfo;
            var ceilingInfo = _ceilingDetector.CollisionInfo;

            float factor = groundInfo.hasContact ? groundInertialFactor : airInertialFactor;

            if (!_isGravityEnabled) {
                _inertialComp = Vector3.Lerp(_inertialComp, Vector3.zero, factor * dt);
                return;
            }

            float y = _inertialComp.y;
            var xz = _inertialComp.WithY(0f);

            xz = Vector3.Lerp(xz, Vector3.zero, factor * dt);
            y += gravityForce * dt;

            if (groundInfo.hasContact) y = Mathf.Max(0f, y);
            if (ceilingInfo.hasContact) y = Mathf.Min(0f, y);

            _inertialComp = xz.WithY(y);
        }
    }

}
