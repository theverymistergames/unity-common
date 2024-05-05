using System;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Triggers {

    public sealed class ExitTrigger : Trigger {

        [SerializeField] private LayerMask _layerMask;
        [SerializeReference] [SubclassSelector] private IActorAction _action;

        public override event Action<Collider> OnTriggered = delegate {  };

        private void OnTriggerExit(Collider other) {
            if (!enabled) return;
            if (!_layerMask.Contains(other.gameObject.layer)) return;
            
            OnTriggered.Invoke(other);
            
            if (_action != null && other.gameObject.TryGetComponent(out IActor actor)) {
                _action.Apply(actor, destroyCancellationToken).Forget();
            }
        }
    }
    
}
