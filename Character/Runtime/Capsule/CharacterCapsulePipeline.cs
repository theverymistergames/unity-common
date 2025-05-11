using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Collisions.Core;
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
        private IRadiusCollisionDetector _groundDetector;
        private IRadiusCollisionDetector _ceilingDetector;

        private Vector3 _headRootInitialPosition;
        private float _initialHeight;

        public void OnAwake(IActor actor) {
            _transform = actor.Transform;
            
            _capsuleCollider = actor.GetComponent<CapsuleCollider>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _ceilingDetector = actor.GetComponent<CharacterCeilingDetector>();

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
            var center = (height - _initialHeight) * Vector3.up;
            var halfCenter = 0.5f * center;
            
            _headRoot.localPosition = center + _headRootInitialPosition;
            
            float prevHeight = _capsuleCollider.height;
            _capsuleCollider.height = height;
            _capsuleCollider.center = halfCenter;

            _groundDetector.OriginOffset = halfCenter;
            _groundDetector.Distance = height * 0.5f - _capsuleCollider.radius;
            
            if (!_groundDetector.HasContact) {
                _transform.position += (prevHeight - height) * _transform.up;
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
            return GetColliderCenterPoint(_capsuleCollider.height * 0.5f);
        }

        private Vector3 GetColliderBottomPoint() {
            return GetColliderCenterPoint(-_capsuleCollider.height * 0.5f);
        }

        private Vector3 GetColliderCenterPoint(float verticalOffset = 0f) {
            return _transform.TransformPoint(_capsuleCollider.center + verticalOffset * Vector3.up);
        }
    }

}
