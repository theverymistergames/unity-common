using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

        public static bool TryGetMinimumDistanceHit(int hitCount, IReadOnlyList<RaycastHit> hits, out RaycastHit hit) {
            hit = default;

            if (hitCount <= 0) return false;

            float minDistance = -1f;
            int hitIndex = -1;

            for (int i = 0; i < hitCount; i++) {
                var nextHit = hits[i];
                float distance = nextHit.distance;

                if (distance <= 0f) continue;

                if (distance < minDistance) {
                    hitIndex = i;
                    minDistance = distance;
                    continue;
                }

                hitIndex = i;
                minDistance = distance;
            }

            if (hitIndex < 0) return false;

            hit = hits[hitIndex];
            return true;
        }

        public static bool TryGetMinimumDistanceHit(int hitCount, IReadOnlyList<RaycastResult> hits, out RaycastResult hit) {
            hit = default;

            if (hitCount <= 0) return false;

            float minDistance = -1f;
            int hitIndex = -1;

            for (int i = 0; i < hitCount; i++) {
                var nextHit = hits[i];
                float distance = nextHit.distance;

                if (distance <= 0f) continue;

                if (distance < minDistance) {
                    hitIndex = i;
                    minDistance = distance;
                    continue;
                }

                hitIndex = i;
                minDistance = distance;
            }

            if (hitIndex < 0) return false;

            hit = hits[hitIndex];
            return true;
        }
    }
    
}
