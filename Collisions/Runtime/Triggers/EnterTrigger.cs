using System;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Triggers {

    public sealed class EnterTrigger : Trigger {

        [SerializeField] private LayerMask _layerMask;

        public override event Action<Collider> OnTriggered = delegate {  };

        private void OnTriggerEnter(Collider other) {
            if (!enabled) return;
            if (_layerMask.Contains(other.gameObject.layer)) OnTriggered.Invoke(other);
        }
    }
    
}
