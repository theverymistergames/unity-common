using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Collisions.Core;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class ActionOnCollision : MonoBehaviour, IActorComponent {

        [SerializeField] private CollisionEmitter _collisionEmitter;
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
            _collisionEmitter.Subscribe(_triggerEvent, HandleCollision);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            _collisionEmitter.Unsubscribe(_triggerEvent, HandleCollision);
        }

        private void HandleCollision(Collision collision) {
            if (!_layerMask.Contains(collision.gameObject.layer)) return;

            _action?.Apply(_actor, _enableCts.Token).Forget();
        }
    }
    
}