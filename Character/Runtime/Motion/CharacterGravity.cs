using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Common.Tick;
using MisterGames.Logic.Phys;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterGravity : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private float _applyFallForceBelowVerticalSpeed;
        [SerializeField] private float _fallForce = 10f;

        public Vector3 GravityDir => _customGravity.GravityDirection;
        public bool UseGravity => _customGravity.UseGravity;
        public bool IsFallForceAllowed { get; set; }
        
        private Rigidbody _rigidbody;
        private RigidbodyCustomGravity _customGravity;
        private CharacterGroundDetector _groundDetector;

        void IActorComponent.OnAwake(IActor actor)
        {
            _rigidbody = actor.GetComponent<Rigidbody>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _customGravity = actor.GetComponent<RigidbodyCustomGravity>();
        }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        public void OnUpdate(float dt) {
            if (!_customGravity.UseGravity || _groundDetector.CollisionInfo.hasContact) return;

            var dir = _customGravity.GravityDirection;
            var velocity = _rigidbody.linearVelocity;
            float fallDir = Mathf.Sign(Vector3.Dot(-dir, velocity));
            
            if (IsFallForceAllowed || 
                fallDir * Vector3.Project(velocity, dir).sqrMagnitude <= 
                _applyFallForceBelowVerticalSpeed * _applyFallForceBelowVerticalSpeed
            ) {
                _rigidbody.AddForce(dir * _fallForce, ForceMode.Acceleration);
            }
        }
    }
    
}