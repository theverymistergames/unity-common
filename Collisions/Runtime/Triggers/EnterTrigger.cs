using System;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Attributes;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Triggers {

    public sealed class EnterTrigger : Trigger {

        [SerializeField] private LayerMask _layerMask;
        [SerializeReference] [SubclassSelector] private IActorAction _action;

        public override event Action<Collider> OnTriggered = delegate {  };

        private void OnTriggerEnter(Collider other) {
            if (!enabled || !_layerMask.Contains(other.gameObject.layer)) return;

            OnTriggered.Invoke(other);
            
            if (_action != null && other.GetComponentFromCollider<IActor>() is {} actor) {
                _action.Apply(actor, destroyCancellationToken).Forget();
            }
        }
    }
    
}
