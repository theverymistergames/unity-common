using MisterGames.Actors;
using MisterGames.Character.Collisions;
using MisterGames.Character.Motion;
using MisterGames.Collisions.Core;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    public sealed class CharacterCapsulePipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private Transform _headRoot;
        
        public event HeightChangeCallback OnHeightChange = delegate { };

        public float Height { get => _characterController.height; set => SetHeight(value); }
        public float Radius { get => _characterController.radius; set => SetRadius(value); }
        public CharacterCapsuleSize CapsuleSize { get => GetCapsuleSize(); set => SetCapsuleSize(value); }

        public Vector3 ColliderTop => GetColliderTopPoint();
        public Vector3 ColliderCenter => GetColliderCenterPoint();
        public Vector3 ColliderBottom => GetColliderBottomPoint();

        public delegate void HeightChangeCallback(float newHeight, float oldHeight);
        
        private CharacterController _characterController;
        private ITransformAdapter _bodyAdapter;
        private IRadiusCollisionDetector _groundDetector;
        private IRadiusCollisionDetector _ceilingDetector;

        private Vector3 _headRootInitialPosition;
        private float _initialHeight;

        public void OnAwake(IActor actor) {
            _bodyAdapter = actor.GetComponent<CharacterBodyAdapter>();
            _characterController = actor.GetComponent<CharacterController>();
            
            var collisionPipeline = actor.GetComponent<CharacterCollisionPipeline>();
            _groundDetector = collisionPipeline.GroundDetector;
            _ceilingDetector = collisionPipeline.CeilingDetector;

            _headRootInitialPosition = _headRoot.localPosition;
            _initialHeight = _characterController.height;
        }

        private void OnDisable() {
            _headRoot.localPosition = _headRootInitialPosition;
        }

        private void SetCapsuleSize(CharacterCapsuleSize capsuleSize) {
            SetHeight(capsuleSize.height);
            SetRadius(capsuleSize.radius);
        }

        private void SetHeight(float height) {
            if (!enabled) return;

            float sourceHeight = _characterController.height;
            ApplyHeight(height);

            if (!height.IsNearlyEqual(sourceHeight)) {
                OnHeightChange.Invoke(height, sourceHeight);
            }
        }

        private void SetRadius(float radius) {
            if (!enabled) return;

            ApplyRadius(radius);
        }

        private void ApplyHeight(float height) {
            var center = (height - _initialHeight) * Vector3.up;
            var halfCenter = 0.5f * center;

            float detectorDistance = height * 0.5f - _characterController.radius;
            float previousHeight = _characterController.height;

            _headRoot.localPosition = center + _headRootInitialPosition;

            _characterController.height = height;
            _characterController.center = halfCenter;

            _groundDetector.OriginOffset = halfCenter;
            _groundDetector.Distance = detectorDistance;
            _groundDetector.FetchResults();

            if (!_groundDetector.CollisionInfo.hasContact) {
                _bodyAdapter.Move(Vector3.up * (previousHeight - height));
            }
        }

        private void ApplyRadius(float radius) {
            _characterController.radius = radius;
            _groundDetector.Radius = radius;
            _ceilingDetector.Radius = radius;
        }

        private CharacterCapsuleSize GetCapsuleSize() {
            return new CharacterCapsuleSize {
                height = _characterController.height,
                radius = _characterController.radius,
            };
        }

        private Vector3 GetColliderTopPoint() {
            return _bodyAdapter.Position + _characterController.center + _characterController.height * 0.5f * Vector3.up;
        }

        private Vector3 GetColliderCenterPoint() {
            return _bodyAdapter.Position + _characterController.center;
        }

        private Vector3 GetColliderBottomPoint() {
            return _bodyAdapter.Position + _characterController.center + _characterController.height * 0.5f * Vector3.down;
        }
    }

}
