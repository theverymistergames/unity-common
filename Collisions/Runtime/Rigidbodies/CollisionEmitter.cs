using System;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public abstract class CollisionEmitter : MonoBehaviour {
        
        public delegate void CollisionCallback(Collision collision);
        
        public abstract event CollisionCallback CollisionEnter;
        public abstract event CollisionCallback CollisionExit;
        public abstract event CollisionCallback CollisionStay;

        public void Subscribe(TriggerEventType evt, CollisionCallback callback) {
            switch (evt) {
                case TriggerEventType.Enter:
                    CollisionEnter += callback;
                    break;
                
                case TriggerEventType.Stay:
                    CollisionStay += callback;
                    break;
                
                case TriggerEventType.Exit:
                    CollisionExit += callback;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(evt), evt, null);
            }
        }

        public void Unsubscribe(TriggerEventType eventType, CollisionCallback callback) {
            switch (eventType) {
                case TriggerEventType.Enter:
                    CollisionEnter -= callback;
                    break;
                
                case TriggerEventType.Stay:
                    CollisionStay -= callback;
                    break;
                
                case TriggerEventType.Exit:
                    CollisionExit -= callback;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
        }
    }
    
}