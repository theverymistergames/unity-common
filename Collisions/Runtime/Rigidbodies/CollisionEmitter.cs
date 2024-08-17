using System;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public abstract class CollisionEmitter : MonoBehaviour {
        
        public delegate void CollisionCallback(Collision collision);
        
        public abstract event CollisionCallback CollisionEnter;
        public abstract event CollisionCallback CollisionExit;
        public abstract event CollisionCallback CollisionStay;

        public void Subscribe(TriggerEvent evt, CollisionCallback callback) {
            switch (evt) {
                case TriggerEvent.Enter:
                    CollisionEnter += callback;
                    break;
                
                case TriggerEvent.Stay:
                    CollisionStay += callback;
                    break;
                
                case TriggerEvent.Exit:
                    CollisionExit += callback;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(evt), evt, null);
            }
        }

        public void Unsubscribe(TriggerEvent eventType, CollisionCallback callback) {
            switch (eventType) {
                case TriggerEvent.Enter:
                    CollisionEnter -= callback;
                    break;
                
                case TriggerEvent.Stay:
                    CollisionStay -= callback;
                    break;
                
                case TriggerEvent.Exit:
                    CollisionExit -= callback;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
    
}