using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class GravityProvider : MonoBehaviour, IUpdate {

        [SerializeField] private Transform _transform;
        [SerializeField] private RigidbodyCustomGravity.Mode _gravityMode = RigidbodyCustomGravity.Mode.CustomGlobalOrPhysics;
        [VisibleIf(nameof(_gravityMode), 3)]
        [SerializeField] private CustomGravitySource _localGravitySource;

        public event Action<Vector3> OnGravityChanged = delegate { };

        public Vector3 GravityDirection { get; private set; } = Vector3.down;
        public float GravityMagnitude { get; private set; } = RigidbodyCustomGravity.GravityMagnitudeDefault;
        
        private Vector3 _lastGravity = Vector3.down * RigidbodyCustomGravity.GravityMagnitudeDefault;

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var gravity = GetGravity(_transform.position);
            
            if (NotifyGravityVector(gravity)) {
                OnGravityChanged.Invoke(gravity);
            }
        }
        
        private Vector3 GetGravity(Vector3 position) {
            return _gravityMode switch {
                RigidbodyCustomGravity.Mode.Physics => Physics.gravity,
                RigidbodyCustomGravity.Mode.CustomGlobalOrPhysics => CustomGravity.Main.TryGetGlobalGravity(position, out var g) ? g : Physics.gravity,
                RigidbodyCustomGravity.Mode.CustomGlobal => CustomGravity.Main.GetGlobalGravity(position),
                RigidbodyCustomGravity.Mode.CustomLocal => _localGravitySource.GetGravity(position, out _),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        private bool NotifyGravityVector(Vector3 gravity) {
            if (gravity == _lastGravity) return false;
            
            _lastGravity = gravity;

            // Do not change last direction if gravity is zero
            if (gravity == default) {
                GravityMagnitude = 0f;
            }
            else {
                GravityDirection = gravity.normalized;
                GravityMagnitude = gravity.magnitude;
            }

            return true;
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _transform = transform;
        }
#endif
    }
    
}