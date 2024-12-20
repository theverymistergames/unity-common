using MisterGames.Actors;
using MisterGames.Character.Collisions;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterExtraGravity : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private float _applyFallForceBelowVerticalSpeed;
        [SerializeField] private float _fallForce = 10f;
        
        public Vector3 GravityDir { get; private set; }
        public bool IsFallForceAllowed { get; set; }

        private Rigidbody _rigidbody;
        private CharacterGroundDetector _groundDetector;
        private Vector3 _lastGravity;

        void IActorComponent.OnAwake(IActor actor)
        {
            _rigidbody = actor.GetComponent<Rigidbody>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            
            _lastGravity = Physics.gravity;
            GravityDir = _lastGravity.normalized;
        }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        public void OnUpdate(float dt) {
            var gravity = Physics.gravity;
            
            if (gravity != _lastGravity) {
                _lastGravity = gravity;
                GravityDir = _lastGravity.normalized;
            }
            
            if (!_rigidbody.useGravity || _groundDetector.CollisionInfo.hasContact) return;
            
            var velocity = _rigidbody.linearVelocity;
            float fallDir = Mathf.Sign(Vector3.Dot(-GravityDir, velocity));
            
            if (IsFallForceAllowed || 
                fallDir * Vector3.Project(velocity, GravityDir).sqrMagnitude <= 
                _applyFallForceBelowVerticalSpeed * _applyFallForceBelowVerticalSpeed
            ) {
                _rigidbody.AddForce(GravityDir * _fallForce, ForceMode.Acceleration);
            }
        }
    }
    
}