﻿using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Common.Tick;
using MisterGames.Logic.Phys;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterGravity : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private float _applyFallForceBelowVerticalSpeed;
        [SerializeField] private float _gravityWeight = 2f;

        public Vector3 Gravity => _customGravity.Gravity;
        public Vector3 GravityDirection => _customGravity.GravityDirection;
        public float GravityMagnitude => _customGravity.GravityMagnitude;
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

            var gravity = _customGravity.Gravity;
            var velocity = _rigidbody.linearVelocity;
            float fallDir = Mathf.Sign(Vector3.Dot(-gravity, velocity));
            
            if (IsFallForceAllowed || 
                fallDir * Vector3.Project(velocity, gravity).sqrMagnitude <= 
                _applyFallForceBelowVerticalSpeed * _applyFallForceBelowVerticalSpeed
            ) {
                _rigidbody.AddForce(gravity * (_gravityWeight - 1f), ForceMode.Acceleration);
            }
        }
    }
    
}