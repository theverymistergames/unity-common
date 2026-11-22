using System.Collections.Generic;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {

    public sealed class TriggerListenerNoStay : TriggerEmitter {

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private bool _collideWithTriggers = true;
        [SerializeField] private bool _clearCollidersOnDisable = true;

        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };

        private readonly HashSet<Collider> _colliderSet = new();
    
        private void OnDisable() {
            if (!_clearCollidersOnDisable) return;

            foreach (var c in _colliderSet) {
                TriggerExit.Invoke(c);
            }

            _colliderSet.Clear();
        }
        
        public override IReadOnlyCollection<Collider> GetEnteredColliders() {
            _colliderSet.RemoveWhere(c => c == null || !c.enabled || c.gameObject is not { activeSelf: true, activeInHierarchy: true });
            return _colliderSet;
        }

        private void OnTriggerEnter(Collider collider) {
            if (!CanCollide(collider)) return;

            if (_colliderSet.Add(collider)) {
                TriggerEnter.Invoke(collider);
            }
        }

        private void OnTriggerExit(Collider collider) {
            if (!_colliderSet.Remove(collider)) return;

            TriggerExit.Invoke(collider);
        }

        private bool CanCollide(Collider collider) {
            return enabled &&
                   (_collideWithTriggers || !collider.isTrigger) &&
                   _layerMask.Contains(collider.gameObject.layer);
        }
    }

}
