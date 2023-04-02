using System;
using MisterGames.Collisions.Utils;
using UnityEngine;

namespace MisterGames.Collisions.Core {

    public abstract class CollisionDetectorBase : MonoBehaviour, ICollisionDetector {

        public event Action OnContact = delegate {  };
        public event Action OnLostContact = delegate {  };
        public event Action OnTransformChanged = delegate {  };

        public CollisionInfo CollisionInfo { get; private set; }

        public abstract void FilterLastResults(CollisionFilter filter, out CollisionInfo info);
        public abstract void FetchResults();

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
            if (forceNotify || newInfo.IsTransformChanged(lastInfo)) OnTransformChanged.Invoke();
        }
    }

}
