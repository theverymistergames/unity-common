using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Character.View;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterWaterProcessor : MonoBehaviour, IActorComponent, IMotionProcessor {
    
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private int _orientationPriority = 1;
        [SerializeField] [Min(0f)] private float _underwaterSpeed = 1f;
        
        private readonly HashSet<Collider> _colliders = new();
        
        private CharacterMotionPipeline _motionPipeline;
        private CharacterViewPipeline _viewPipeline;
        private CharacterGroundDetector _groundDetector;
        
        void IActorComponent.OnAwake(IActor actor) {
            _motionPipeline  = actor.GetComponent<CharacterMotionPipeline>();
            _viewPipeline = actor.GetComponent<CharacterViewPipeline>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
        }

        private void OnEnable() {
            _motionPipeline.AddProcessor(this);
            
            _triggerEmitter.TriggerEnter += TriggerEnter;
            _triggerEmitter.TriggerExit += TriggerExit;
        }

        private void OnDisable() {
            _motionPipeline.RemoveProcessor(this);
            
            _triggerEmitter.TriggerEnter -= TriggerEnter;
            _triggerEmitter.TriggerExit -= TriggerExit;
        }

        private void TriggerEnter(Collider collider) {
            if (_layerMask.Contains(collider.gameObject.layer)) _colliders.Add(collider);
        }
        
        private void TriggerExit(Collider collider) {
            _colliders.Remove(collider);
        }
        
        bool IMotionProcessor.ProcessOrientation(ref Quaternion orientation, out int priority) {
            priority = _orientationPriority;
            if (!InWaterColliders()) return false;
            
            orientation = _viewPipeline.HeadRotation;
            return true;
        }

        void IMotionProcessor.ProcessInputSpeed(ref float speed, float dt) {
            if (!InWaterColliders()) return;

            speed = _underwaterSpeed;
        }

        void IMotionProcessor.ProcessInputForce(ref Vector3 inputForce, Vector3 desiredVelocity, float dt) {
            
        }

        private bool InWaterColliders() {
            return _colliders.Count > 0;
        }
    }
    
}