using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class ActionOnTriggerForRigidbody : MonoBehaviour, IActorComponent {

        [SerializeField] private TriggerListenerForRigidbody _triggerListenerForRigidbody;
        [SerializeField] private LayerMask _layerMask;
        [SerializeReference] [SubclassSelector] private IActorAction _onEnter;
        [SerializeReference] [SubclassSelector] private IActorAction _onExit;

        private CancellationTokenSource _enableCts;
        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _triggerListenerForRigidbody.TriggerEnter += TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit += TriggerExit;
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _triggerListenerForRigidbody.TriggerEnter -= TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit -= TriggerExit;
        }

        private void TriggerEnter(Rigidbody rigidbody) {
            if (rigidbody == null || !_layerMask.Contains(rigidbody.gameObject.layer)) return;

            _onEnter?.Apply(_actor, _enableCts.Token).Forget();
        }

        private void TriggerExit(Rigidbody rigidbody) {
            if (rigidbody == null || !_layerMask.Contains(rigidbody.gameObject.layer)) return;

            _onExit?.Apply(_actor, _enableCts.Token).Forget();   
        }
    }
    
}