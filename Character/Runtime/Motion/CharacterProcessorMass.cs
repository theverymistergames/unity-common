using System;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Character.Processors;
using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Motion {

    [Serializable]
    public sealed class CharacterProcessorMass : ICharacterProcessorVector3, ICharacterAccessInitializable {

        [Header("Inertia")]
        public float airInertialFactor = 1f;
        public float groundInertialFactor = 1f;
        public float forceInfluenceFactor = 1f;

        [Header("Gravity")]
        public float gravityForce = 9.8f;
        public bool isGravityEnabled = true;

        private ICollisionDetector _groundDetector;
        private ICollisionDetector _ceilingDetector;
        private ICollisionDetector _hitDetector;

        private Vector3 _inertialComponent;
        private Vector3 _gravitationalComponent;

        private Vector3 _lastGroundNormal;
        private Vector3 _lastCeilingNormal;

        private Vector3 _previousVelocity;
        private Vector3 _currentVelocity;

        public void Initialize(ICharacterAccess characterAccess) {
            var collisionPipeline = characterAccess.GetPipeline<ICharacterCollisionPipeline>();

            _hitDetector = collisionPipeline.HitDetector;
            _ceilingDetector = collisionPipeline.CeilingDetector;
            _groundDetector = collisionPipeline.GroundDetector;

            _hitDetector.OnTransformChanged -= OnHitDetectorTransformChanged;
            _hitDetector.OnTransformChanged += OnHitDetectorTransformChanged;
        }

        public void DeInitialize() {
            _hitDetector.OnTransformChanged -= OnHitDetectorTransformChanged;
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

            return _currentVelocity;
        }

        private void OnHitDetectorTransformChanged() {
            var info = _hitDetector.CollisionInfo;
            if (!info.hasContact) return;

            _inertialComponent = Vector3.ProjectOnPlane(_inertialComponent, info.normal);
            _gravitationalComponent = Vector3.ProjectOnPlane(_gravitationalComponent, info.normal);
        }

        /// <summary>
        /// Interpolates value of the inertial component towards current force vector
        /// with ground or in-air inertial factor.
        /// </summary>
        private void UpdateInertialComponent(Vector3 force, float dt) {
            var groundInfo = _groundDetector.CollisionInfo;
            var ceilingInfo = _ceilingDetector.CollisionInfo;

            float factor = groundInfo.hasContact
                ? groundInertialFactor
                : airInertialFactor * GetForceInfluence(force, _inertialComponent, forceInfluenceFactor);

            _inertialComponent = Vector3.Lerp(_inertialComponent, force, factor * dt);

            if (groundInfo.hasContact && !groundInfo.normal.IsNearlyEqual(_lastGroundNormal)) {
                _inertialComponent = Vector3.ProjectOnPlane(_inertialComponent, groundInfo.normal);
            }

            if (ceilingInfo.hasContact && !ceilingInfo.normal.IsNearlyEqual(_lastCeilingNormal)) {
                _inertialComponent = Vector3.ProjectOnPlane(_inertialComponent, ceilingInfo.normal);
            }

            _lastGroundNormal = groundInfo.hasContact ? groundInfo.normal : Vector3.up;
            _lastCeilingNormal = ceilingInfo.hasContact ? ceilingInfo.normal : Vector3.up;
        }

        /// <summary>
        /// Interpolates value of the gravitational component:
        ///
        /// 1) If gravity is enabled and character is not grounded (i.e. is falling down) -
        ///    gravitational component is increased by gravity force per frame.
        ///
        /// 2) If gravity is not enabled or character is grounded -
        ///    gravitational component is interpolated towards Vector3.zero with ground or in-air inertial factor.
        ///
        /// </summary>
        private void UpdateGravitationalComponent(float dt) {
            var groundInfo = _groundDetector.CollisionInfo;
            var ceilingInfo = _ceilingDetector.CollisionInfo;

            if (isGravityEnabled && !groundInfo.hasContact) {
                _gravitationalComponent += gravityForce * dt * Vector3.down;
            }
            else {
                float factor = groundInfo.hasContact ? groundInertialFactor : airInertialFactor;
                _gravitationalComponent = Vector3.Lerp(_gravitationalComponent, Vector3.zero, factor * dt);
            }

            if (groundInfo.hasContact && !groundInfo.normal.IsNearlyEqual(_lastGroundNormal)) {
                _gravitationalComponent = Vector3.ProjectOnPlane(_gravitationalComponent, groundInfo.normal);
            }

            if (ceilingInfo.hasContact && !ceilingInfo.normal.IsNearlyEqual(_lastCeilingNormal)) {
                _gravitationalComponent = Vector3.ProjectOnPlane(_gravitationalComponent, ceilingInfo.normal);
            }

            _lastGroundNormal = groundInfo.hasContact ? groundInfo.normal : Vector3.up;
            _lastCeilingNormal = ceilingInfo.hasContact ? ceilingInfo.normal : Vector3.up;
        }

        /// <summary>
        /// Force influence allows to save the bigger amount of the inertial energy while not grounded,
        /// the greater the inertial component value compared to force value:
        ///
        /// 1) Value is a relation of force magnitude to inertial component magnitude,
        ///    if inertial component is bigger than force component.
        ///
        /// 2) Value is 1f (no influence),
        ///    if inertial component is equal or less than force component.
        ///
        /// </summary>
        private static float GetForceInfluence(Vector3 force, Vector3 inertialComponent, float multiplier) {
            float inertialSqrMagnitude = inertialComponent.sqrMagnitude;
            float forceSqrMagnitude = force.sqrMagnitude;

            return inertialSqrMagnitude > forceSqrMagnitude
                ? multiplier * forceSqrMagnitude / inertialSqrMagnitude
                : 1f;
        }
    }

}
