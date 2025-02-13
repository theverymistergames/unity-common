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
    
    public sealed class ActionOnTrigger : MonoBehaviour, IActorComponent {

        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private TriggerEventType _triggerEvent; 
        [SerializeField] private LayerMask _layerMask;
        [SerializeReference] [SubclassSelector] private IActorAction _action;

        private CancellationTokenSource _enableCts;
        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            _triggerEmitter.Subscribe(_triggerEvent, HandleTrigger);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            _triggerEmitter.Unsubscribe(_triggerEvent, HandleTrigger);
        }

        private void HandleTrigger(Collider collider) {
            if (!_layerMask.Contains(collider.gameObject.layer)) return;

            _action?.Apply(_actor, _enableCts.Token).Forget();
        }
    }
    
}