using System;
using MisterGames.Collisions.Utils;
using UnityEngine;

namespace MisterGames.Collisions.Core {

    public abstract class CollisionDetectorBase : MonoBehaviour, ICollisionDetector {

        public event Action OnContact = delegate {  };
        public event Action OnLostContact = delegate {  };
        public event Action OnTransformChanged = delegate {  };

        public abstract Vector3 OriginOffset { get; set; }
        public abstract float Distance { get; set; }
        public abstract int Capacity { get; }
        public bool HasContact { get; private set; }
        public CollisionInfo CollisionInfo { get; private set; }

        public abstract ReadOnlySpan<CollisionInfo> FilterLastResults(CollisionFilter filter);
        public abstract void FetchResults();

        protected void SetCollisionInfo(CollisionInfo newInfo, bool forceNotify = false) {
            var lastInfo = CollisionInfo;
            CollisionInfo = newInfo;

            HasContact = newInfo.hasContact;
            
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
