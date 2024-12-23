using System;
using MisterGames.Actors;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Attributes;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Logic.Damage {
    
    public sealed class DamageOnCollision : MonoBehaviour, IActorComponent {
        
        [Header("Source of Damage")]
        [SerializeField] private EventSource _eventSource;
        [VisibleIf("_eventSource", 0)] 
        [SerializeField] private CollisionEmitter _collisionEmitter;
        [VisibleIf("_eventSource", 1)] 
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private TriggerEventType _eventType = TriggerEventType.Enter;
        [SerializeField] private LayerMask _layerMask;
        
        [Header("Damage Parameters")]
        [SerializeField] private DamageDealer _damageDealer = DamageDealer.Actor;
        [SerializeField] [Min(0f)] private float _damage = 1f;
        [SerializeField] private bool _disableAfterFirstDamage = true;
        
        private enum EventSource {
            Collision,
            Trigger,
        }

        private enum DamageDealer {
            Actor,
            ParentActor,
        }
        
        private IActor _actor;
        private bool _didDamage;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnEnable() {
            _didDamage = false;
            
            switch (_eventSource) {
                case EventSource.Collision:
                    _collisionEmitter.Subscribe(_eventType, HandleCollision);
                    break;
                
                case EventSource.Trigger:
                    _triggerEmitter.Subscribe(_eventType, HandleTrigger);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDisable() {
            switch (_eventSource) {
                case EventSource.Collision:
                    _collisionEmitter.Unsubscribe(_eventType, HandleCollision);
                    break;
                
                case EventSource.Trigger:
                    _triggerEmitter.Unsubscribe(_eventType, HandleTrigger);
                    break;
             
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void HandleCollision(Collision collision) {
            if (_didDamage && _disableAfterFirstDamage ||
                !_layerMask.Contains(collision.gameObject.layer) ||
                collision.GetComponentFromCollision<IActor>() is not { } actor ||
                !actor.TryGetComponent(out HealthBehaviour healthBehaviour)) 
            {
                return;
            }
            
            ApplyDamage(healthBehaviour, collision.GetContact(0).point);
        }
        
        private void HandleTrigger(Collider collider) {
            if (_didDamage && _disableAfterFirstDamage ||
                !_layerMask.Contains(collider.gameObject.layer) ||
                collider.GetComponentFromCollider<IActor>() is not { } actor ||
                !actor.TryGetComponent(out HealthBehaviour healthBehaviour)) 
            {
                return;
            }
            
            ApplyDamage(healthBehaviour, collider.transform.position);
        }

        private void ApplyDamage(HealthBehaviour healthBehaviour, Vector3 point) {
            var author = _damageDealer switch {
                DamageDealer.Actor => _actor,
                DamageDealer.ParentActor => _actor?.ParentActor,
                _ => throw new ArgumentOutOfRangeException(),
            };

            healthBehaviour.TakeDamage(_damage, author, point);
        }
    }
    
}