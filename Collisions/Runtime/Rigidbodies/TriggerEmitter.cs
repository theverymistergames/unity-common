using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public abstract class TriggerEmitter : MonoBehaviour {

        public delegate void TriggerCallback(Collider collider);
        
        public abstract event TriggerCallback TriggerEnter;
        public abstract event TriggerCallback TriggerExit;
        public abstract event TriggerCallback TriggerStay;

        public abstract IReadOnlyCollection<Collider> EnteredColliders { get; } 
        
        public void Subscribe(TriggerEventType evt, TriggerCallback callback) {
            switch (evt) {
                case TriggerEventType.Enter:
                    TriggerEnter += callback;
                    break;
                
                case TriggerEventType.Stay:
                    TriggerStay += callback;
                    break;
                
                case TriggerEventType.Exit:
                    TriggerExit += callback;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(evt), evt, null);
            }
        }

        public void Unsubscribe(TriggerEventType eventType, TriggerCallback callback) {
            switch (eventType) {
                case TriggerEventType.Enter:
                    TriggerEnter -= callback;
                    break;
                
                case TriggerEventType.Stay:
                    TriggerStay -= callback;
                    break;
                
                case TriggerEventType.Exit:
                    TriggerExit -= callback;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
    
}