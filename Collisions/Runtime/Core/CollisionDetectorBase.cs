using System;
using System.Runtime.CompilerServices;
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

        protected void SetCollisionInfo(CollisionInfo newInfo, bool forceNotify = false) {
            var lastInfo = CollisionInfo;
            CollisionInfo = newInfo;
            HasContact = newInfo.hasContact;
            
            CheckContactChanged(ref lastInfo, ref newInfo, forceNotify);
            
            if (forceNotify || IsTransformChanged(ref newInfo, ref lastInfo)) {
                OnTransformChanged.Invoke();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckContactChanged(ref CollisionInfo lastInfo, ref CollisionInfo newInfo, bool forceNotify) {
            if ((lastInfo.hasContact || forceNotify) && !newInfo.hasContact) {
                OnLostContact.Invoke();
                return;
            }
            
            if ((!lastInfo.hasContact || forceNotify) && newInfo.hasContact) {
                OnContact.Invoke();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTransformChanged(ref CollisionInfo newInfo, ref CollisionInfo lastInfo) {
            return
                !lastInfo.hasContact && newInfo.hasContact ||
                lastInfo.hasContact && !newInfo.hasContact ||
                lastInfo.hasContact && lastInfo.transform.GetInstanceID() != newInfo.transform.GetInstanceID();
        }
    }

}
