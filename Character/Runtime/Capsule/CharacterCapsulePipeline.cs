using MisterGames.Actors;
using MisterGames.Character.Collisions;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Collisions.Core;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    public sealed class CharacterCapsulePipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private Transform _headRoot;
        
        public event HeightChangeCallback OnHeightChange = delegate { };

        public float Height { get => _capsuleCollider.height; set => SetHeight(value); }
        public float Radius { get => _capsuleCollider.radius; set => SetRadius(value); }
        public CharacterCapsuleSize CapsuleSize { get => GetCapsuleSize(); set => SetCapsuleSize(value); }

        public Vector3 ColliderTop => GetColliderTopPoint();
        public Vector3 ColliderCenter => GetColliderCenterPoint();
        public Vector3 ColliderBottom => GetColliderBottomPoint();

        public delegate void HeightChangeCallback(float newHeight, float oldHeight);

        private Transform _transform;
        private CapsuleCollider _capsuleCollider;
        private CharacterViewPipeline _view;
        private IRadiusCollisionDetector _groundDetector;
        private IRadiusCollisionDetector _ceilingDetector;

        private Vector3 _headRootInitialPosition;
        private float _initialHeight;

        public void OnAwake(IActor actor) {
            _transform = actor.Transform;
                
            _view = actor.GetComponent<CharacterViewPipeline>();
            _capsuleCollider = actor.GetComponent<CapsuleCollider>();
            
            var collisionPipeline = actor.GetComponent<CharacterCollisionPipeline>();
            _groundDetector = collisionPipeline.GroundDetector;
            _ceilingDetector = collisionPipeline.CeilingDetector;

            _headRootInitialPosition = _headRoot.localPosition;
            _initialHeight = _capsuleCollider.height;
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

            float sourceHeight = _capsuleCollider.height;
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
            var up = _transform.up;
            var center = (height - _initialHeight) * up;
            var halfCenter = 0.5f * center;
            
            _headRoot.localPosition = center + _headRootInitialPosition;
            
            float prevHeight = _capsuleCollider.height;
            _capsuleCollider.height = height;
            _capsuleCollider.center = halfCenter;

            _groundDetector.OriginOffset = halfCenter;
            _groundDetector.Distance = height * 0.5f - _capsuleCollider.radius;
            
            if (!_groundDetector.HasContact) {
                _view.BodyPosition += (prevHeight - height) * up;
            }
        }

        private void ApplyRadius(float radius) {
            _capsuleCollider.radius = radius;
            _groundDetector.Radius = radius;
            _ceilingDetector.Radius = radius;
        }

        private CharacterCapsuleSize GetCapsuleSize() {
            return new CharacterCapsuleSize {
                height = _capsuleCollider.height,
                radius = _capsuleCollider.radius,
            };
        }

        private Vector3 GetColliderTopPoint() {
            return _view.BodyPosition + _capsuleCollider.center + _capsuleCollider.height * 0.5f * Vector3.up;
        }

        private Vector3 GetColliderCenterPoint() {
            return _view.BodyPosition + _capsuleCollider.center;
        }

        private Vector3 GetColliderBottomPoint() {
            return _view.BodyPosition + _capsuleCollider.center + _capsuleCollider.height * 0.5f * Vector3.down;
        }
    }

}
