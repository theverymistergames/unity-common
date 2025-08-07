using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Capsule {

    public sealed class CharacterCapsulePipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private Transform _headRoot;
        
        public delegate void HeightChangeCallback(float newHeight, float oldHeight);
        public event HeightChangeCallback OnHeightChange = delegate { };

        public Transform Root { get; private set; }
        public float Height { get => _capsuleCollider.height; set => SetHeight(value); }
        public float Radius { get => _capsuleCollider.radius; set => SetRadius(value); }
        public CapsuleSize CapsuleSize { get => GetCapsuleSize(); set => SetCapsuleSize(value); }

        private CapsuleCollider _capsuleCollider;
        private IRadiusCollisionDetector _groundDetector;
        private IRadiusCollisionDetector _ceilingDetector;

        private Vector3 _headRootInitialPosition;
        private float _initialHeight;

        void IActorComponent.OnAwake(IActor actor) {
            Root = actor.Transform;
            
            _capsuleCollider = actor.GetComponent<CapsuleCollider>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _ceilingDetector = actor.GetComponent<CharacterCeilingDetector>();

            _headRootInitialPosition = _headRoot.localPosition;
            _initialHeight = _capsuleCollider.height;
        }

        private void OnDisable() {
            _headRoot.localPosition = _headRootInitialPosition;
        }

        private void SetCapsuleSize(CapsuleSize capsuleSize) {
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
                Root.position += (prevHeight - height) * Root.up;
            }
        }

        private void ApplyRadius(float radius) {
            _capsuleCollider.radius = radius;
            _groundDetector.Radius = radius;
            _ceilingDetector.Radius = radius;
        }

        private CapsuleSize GetCapsuleSize() {
            return new CapsuleSize {
                height = _capsuleCollider.height,
                radius = _capsuleCollider.radius,
            };
        }

        public Vector3 GetColliderTopPoint(float offset = 0f) {
            return GetColliderCenterPoint(_capsuleCollider.height * 0.5f + offset);
        }

        public Vector3 GetColliderBottomPoint(float offset = 0f) {
            return GetColliderCenterPoint(-_capsuleCollider.height * 0.5f + offset);
        }

        public Vector3 GetColliderCenterPoint(float offset = 0f) {
            return Root.TransformPoint(_capsuleCollider.center + offset * Vector3.up);
        }
    }

}
