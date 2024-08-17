using System;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public abstract class TriggerEmitter : MonoBehaviour {

        public delegate void TriggerCallback(Collider collider);
        
        public abstract event TriggerCallback TriggerEnter;
        public abstract event TriggerCallback TriggerExit;
        public abstract event TriggerCallback TriggerStay;

        public void Subscribe(TriggerEvent evt, TriggerCallback callback) {
            switch (evt) {
                case TriggerEvent.Enter:
                    TriggerEnter += callback;
                    break;
                
                case TriggerEvent.Stay:
                    TriggerStay += callback;
                    break;
                
                case TriggerEvent.Exit:
                    TriggerExit += callback;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(evt), evt, null);
            }
        }

        public void Unsubscribe(TriggerEvent eventType, TriggerCallback callback) {
            switch (eventType) {
                case TriggerEvent.Enter:
                    TriggerEnter -= callback;
                    break;
                
                case TriggerEvent.Stay:
                    TriggerStay -= callback;
                    break;
                
                case TriggerEvent.Exit:
                    TriggerExit -= callback;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
    
}