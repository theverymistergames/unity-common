using System;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Triggers {

    public sealed class ExitTrigger : Trigger {

        [SerializeField] private LayerMask _layerMask;

        public override event Action OnTriggered = delegate {  };

        private void OnTriggerExit(Collider other) {
            if (!enabled || !_layerMask.Contains(other.gameObject.layer)) return;
            
            OnTriggered.Invoke();
        }
    }
    
}
