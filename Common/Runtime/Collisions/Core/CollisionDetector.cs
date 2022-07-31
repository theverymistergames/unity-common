using System;
using UnityEngine;

namespace MisterGames.Common.Collisions.Core {

    public abstract class CollisionDetector : MonoBehaviour {

        public event Action OnContact = delegate {  };
        public event Action OnLostContact = delegate {  };
        public event Action OnTransformChanged = delegate {  };

        public CollisionInfo CollisionInfo { get; private set; }

        public abstract void FilterLastResults(CollisionFilter filter, out CollisionInfo info);

        protected void SetCollisionInfo(CollisionInfo newInfo, bool forceNotify = false) {
            var lastInfo = CollisionInfo;
            CollisionInfo = newInfo;
            
            CheckContactChanged(lastInfo, newInfo, forceNotify);
            CheckTransformChanged(lastInfo, newInfo, forceNotify);
        }

        private void CheckContactChanged(CollisionInfo lastInfo, CollisionInfo newInfo, bool forceNotify) {
            bool hadContact = lastInfo.hasContact;
            bool hasContact = newInfo.hasContact;

            if ((hadContact || forceNotify) && !hasContact) {
                OnLostContact.Invoke();
                return;
            }
            
            if ((!hadContact || forceNotify) && hasContact) {
                OnContact.Invoke();
            }
        }

        private void CheckTransformChanged(CollisionInfo lastInfo, CollisionInfo newInfo, bool forceNotify) {
            bool surfaceChanged =
                !lastInfo.hasContact && newInfo.hasContact ||
                lastInfo.hasContact && !newInfo.hasContact ||
                lastInfo.hasContact && lastInfo.transform.GetHashCode() != newInfo.transform.GetHashCode();
            
            if (forceNotify || surfaceChanged) {
                OnTransformChanged.Invoke();
            }
        }
    }

}
