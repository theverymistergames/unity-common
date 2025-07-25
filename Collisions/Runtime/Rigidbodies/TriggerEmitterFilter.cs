using System.Collections.Generic;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class TriggerEmitterFilter : TriggerEmitter {
        
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private LayerMask _layerMask;
        
        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };
        public override event TriggerCallback TriggerStay = delegate { };

        public override IReadOnlyCollection<Collider> EnteredColliders => GetEnteredColliders();
        private readonly HashSet<Collider> _enteredColliders = new();
        
        private void OnEnable() {
            _triggerEmitter.TriggerEnter += HandleTriggerEnter;
            _triggerEmitter.TriggerStay += HandleTriggerStay;
            _triggerEmitter.TriggerExit += HandleTriggerExit;

            FilterColliders();
        }

        private void OnDisable() {
            _triggerEmitter.TriggerEnter -= HandleTriggerEnter;
            _triggerEmitter.TriggerStay -= HandleTriggerStay;
            _triggerEmitter.TriggerExit -= HandleTriggerExit;
        }

        private IReadOnlyCollection<Collider> GetEnteredColliders() {
            if (!enabled) FilterColliders();
            return _enteredColliders;
        }

        private void FilterColliders() {
            _enteredColliders.Clear();
            foreach (var collider in _triggerEmitter.EnteredColliders) {
                if (collider != null && CanCollide(collider)) _enteredColliders.Add(collider);
            }
        }

        private void HandleTriggerEnter(Collider collider) {
            if (!CanCollide(collider)) return;
            
            _enteredColliders.Add(collider);
            TriggerEnter.Invoke(collider);
        }

        private void HandleTriggerStay(Collider collider) {
            if (!CanCollide(collider)) return;
            
            TriggerStay.Invoke(collider);
        }

        private void HandleTriggerExit(Collider collider) {
            if (!_enteredColliders.Remove(collider)) return;
            
            TriggerExit.Invoke(collider);
        }

        private bool CanCollide(Collider collider) {
            return _layerMask.Contains(collider.gameObject.layer);
        }
    }
    
}