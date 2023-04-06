using System;
using MisterGames.Character.Core2.Processors;
using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Core2.Motion {

    [Serializable]
    public sealed class CharacterProcessorMass : ICharacterProcessorVector3, ICharacterProcessorInitializable {

        [Header("Inertia")]
        public float airInertialFactor = 1f;
        public float groundInertialFactor = 1f;

        [Header("Gravity")]
        public float gravityForce = 9.8f;
        public bool isGravityEnabled = true;

        private ICharacterAccess _characterAccess;

        private ICollisionDetector _groundDetector;
        private ICollisionDetector _ceilingDetector;
        private ICollisionDetector _hitDetector;

        private Vector3 _inertialComponent;
        private Vector3 _gravitationalComponent;

        private Vector3 _previousVelocity;
        private Vector3 _currentVelocity;

        public void Initialize(ICharacterAccess characterAccess) {
            _characterAccess = characterAccess;

            _hitDetector = characterAccess.HitDetector;
            _ceilingDetector = characterAccess.CeilingDetector;
            _groundDetector = characterAccess.GroundDetector;

            _hitDetector.OnContact -= HandleHit;
            _hitDetector.OnContact += HandleHit;
        }

        public void DeInitialize() {
            _hitDetector.OnContact -= HandleHit;
        }

        public void ApplyImpulse(Vector3 impulse) {
            _inertialComponent += impulse.WithY(0f);
            _gravitationalComponent.y += impulse.y;
        }

        public void ApplyVelocityChange(Vector3 impulse) {
            _inertialComponent = impulse.WithY(0f);
            _gravitationalComponent.y = impulse.y;
        }

        public Vector3 Process(Vector3 input, float dt) {
            _hitDetector.FetchResults();
            _ceilingDetector.FetchResults();
            _groundDetector.FetchResults();

            UpdateInertialComponent(input, dt);
            UpdateGravitationalComponent(dt);

            _previousVelocity = _currentVelocity;
            _currentVelocity = _gravitationalComponent + _inertialComponent;

            Debug.DrawRay(_characterAccess.BodyAdapter.Position, _currentVelocity, Color.blue);

            return _currentVelocity;
        }

        private void HandleHit() {
            _inertialComponent = Vector3.ProjectOnPlane(_inertialComponent, _hitDetector.CollisionInfo.lastNormal);
        }

        private void UpdateInertialComponent(Vector3 target, float dt) {
            float factor = _groundDetector.CollisionInfo.hasContact ? groundInertialFactor : airInertialFactor;
            _inertialComponent = Vector3.Lerp(_inertialComponent, target, factor * dt);
        }

        private void UpdateGravitationalComponent(float dt) {
            if (isGravityEnabled) {
                _gravitationalComponent += Vector3.down * (gravityForce * dt);
            }
            else {
                float factor = _groundDetector.CollisionInfo.hasContact ? groundInertialFactor : airInertialFactor;
                _gravitationalComponent = Vector3.Lerp(_gravitationalComponent, Vector3.zero, factor * dt);
            }

            if (_groundDetector.CollisionInfo.hasContact) _gravitationalComponent.y = Mathf.Max(0f, _gravitationalComponent.y);
            if (_ceilingDetector.CollisionInfo.hasContact) _gravitationalComponent.y = Mathf.Min(0f, _gravitationalComponent.y);
        }
    }

}
