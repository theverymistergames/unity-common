using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Collisions.Core;
using MisterGames.Common.Attributes;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class ActionOnCollision : MonoBehaviour, IActorComponent {

        [SerializeField] private CollisionEmitter _collisionEmitter;
        [SerializeField] private TriggerEventType _triggerEvent; 
        [SerializeField] private LayerMask _layerMask;
        [SubclassSelector]
        [SerializeReference] private IActorAction _action;
        
        private IActor _actor;
        
        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnEnable() {
            _collisionEmitter.Subscribe(_triggerEvent, HandleCollision);
        }

        private void OnDisable() {
            _collisionEmitter.Unsubscribe(_triggerEvent, HandleCollision);
        }

        private void HandleCollision(Collision collision) {
            if (!_layerMask.Contains(collision.gameObject.layer)) return;

            _action?.Apply(_actor, destroyCancellationToken).Forget();
        }
    }
    
}