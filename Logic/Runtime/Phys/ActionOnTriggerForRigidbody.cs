using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class ActionOnTriggerForRigidbody : MonoBehaviour, IActorComponent {

        [SerializeField] private TriggerListenerForRigidbody _triggerListenerForRigidbody;
        [SerializeField] private RigidbodyTriggerEventType _triggerEvent; 
        [SerializeField] private LayerMask _layerMask;
        [SerializeReference] [SubclassSelector] private IActorAction _action;

        private CancellationTokenSource _enableCts;
        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            _triggerListenerForRigidbody.Subscribe(_triggerEvent, HandleTrigger);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            _triggerListenerForRigidbody.Unsubscribe(_triggerEvent, HandleTrigger);
        }

        private void HandleTrigger(Rigidbody rigidbody) {
            if (rigidbody == null || !_layerMask.Contains(rigidbody.gameObject.layer)) return;

            _action?.Apply(_actor, _enableCts.Token).Forget();
        }
    }
    
}