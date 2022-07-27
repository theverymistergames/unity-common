using UnityEngine;

namespace MisterGames.Common.Collisions {
    
    public static class CollisionUtils {

        public static bool HasLostContact(CollisionInfo lastInfo, CollisionInfo newInfo) {
            return lastInfo.hasContact && !newInfo.hasContact;
        }

        public static bool HasNewContact(CollisionInfo lastInfo, CollisionInfo newInfo) {
            return !lastInfo.hasContact && newInfo.hasContact;
        }

        public static bool HasContactWithTransform(this CollisionInfo info, Transform transform) {
            return info.hasContact && info.transform.GetHashCode() == transform.GetHashCode();
        }

        public static bool IsTransformChanged(CollisionInfo lastInfo, CollisionInfo newInfo) {
            return
                !lastInfo.hasContact && newInfo.hasContact ||
                lastInfo.hasContact && !newInfo.hasContact ||
                lastInfo.hasContact && lastInfo.transform.GetHashCode() != newInfo.transform.GetHashCode();
        }

        public static bool Contains(this LayerMask mask, int layer) {
            return mask == (mask | (1 << layer));
        }
        
    }
    
}
