using System;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Triggers {

    public sealed class ExitTrigger : Trigger {

        [SerializeField] private LayerMask _layerMask;

        public override event Action<GameObject> OnTriggered = delegate {  };

        private void OnTriggerExit(Collider other) {
            if (!enabled) return;

            var go = other.gameObject;
            if (_layerMask.Contains(go.layer)) OnTriggered.Invoke(go);
        }
    }
    
}
