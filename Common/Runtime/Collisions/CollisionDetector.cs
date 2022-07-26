using System;
using UnityEngine;

namespace MisterGames.Common.Collisions {

    public abstract class CollisionDetector : MonoBehaviour {

        public event Action OnContact = delegate {  };
        public event Action OnLostContact = delegate {  };
        public event Action OnSurfaceChanged = delegate {  };

        public CollisionInfo CollisionInfo { get; private set; }

        protected void SetCollisionInfo(CollisionInfo newInfo, bool forceNotify = false) {
            var lastInfo = CollisionInfo;
            CollisionInfo = newInfo;
            
            CheckContact(lastInfo, newInfo, forceNotify);
            CheckSurface(lastInfo, newInfo, forceNotify);
        }

        private void CheckContact(CollisionInfo lastInfo, CollisionInfo newInfo, bool forceNotify) {
            var hadContact = lastInfo.hasContact;
            var hasContact = newInfo.hasContact;

            if ((hadContact || forceNotify) && !hasContact) {
                OnLostContact.Invoke();
                return;
            }
            
            if ((!hadContact || forceNotify) && hasContact) {
                OnContact.Invoke();
            }
        }

        private void CheckSurface(CollisionInfo lastInfo, CollisionInfo newInfo, bool forceNotify) {
            var surfaceChanged = forceNotify ||
                !lastInfo.hasContact && newInfo.hasContact ||
                lastInfo.hasContact && !newInfo.hasContact ||
                lastInfo.hasContact && lastInfo.surface.GetHashCode() != newInfo.surface.GetHashCode();
            
            if (surfaceChanged) {
                OnSurfaceChanged.Invoke();
            }
        }

    }

}