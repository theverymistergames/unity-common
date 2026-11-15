using System.Collections.Generic;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {

    public sealed class TriggerListenerNoStay : TriggerEmitter {

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private bool _collideWithTriggers = true;
        [SerializeField] private bool _clearCollidersOnDisable = true;

        public override event TriggerCallback TriggerEnter = delegate { };
        public override event TriggerCallback TriggerExit = delegate { };

        public override IReadOnlyCollection<Collider> EnteredColliders => _colliderSet;
        private readonly HashSet<Collider> _colliderSet = new();
    
        private void OnDisable() {
            if (!_clearCollidersOnDisable) return;

            foreach (var c in _colliderSet) {
                TriggerExit.Invoke(c);
            }

            _colliderSet.Clear();
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
                   _layerMask.Contains(collider.gameObject.layer) &&
                   (_collideWithTriggers || !collider.isTrigger);
        }
    }

}
