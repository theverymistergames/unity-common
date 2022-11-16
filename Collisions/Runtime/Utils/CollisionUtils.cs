using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Collisions.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.Collisions.Utils {
    
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

        public static bool IsTransformChanged(this CollisionInfo newInfo, CollisionInfo lastInfo) {
            return
                !lastInfo.hasContact && newInfo.hasContact ||
                lastInfo.hasContact && !newInfo.hasContact ||
                lastInfo.hasContact && lastInfo.transform.GetHashCode() != newInfo.transform.GetHashCode();
        }

        public static bool Contains(this LayerMask mask, int layer) {
            return mask == (mask | (1 << layer));
        }

        public static IList<RaycastHit> RemoveInvalidHits(
            this IList<RaycastHit> hits,
            int hitCount,
            out int resultHitCount)
        {
            hitCount = Math.Min(hitCount, hits.Count);
            resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var currentHit = hits[i];
                if (currentHit.distance > 0f) continue;

                int lastValidHitIndex = --resultHitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = currentHit;
            }

            return hits;
        }

        public static IList<RaycastResult> RemoveInvalidHits(
            this IList<RaycastResult> hits,
            int hitCount,
            out int resultHitCount)
        {
            hitCount = Math.Min(hitCount, hits.Count);
            resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var currentHit = hits[i];
                if (currentHit.distance > 0f) continue;

                int lastValidHitIndex = --resultHitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = currentHit;
            }

            return hits;
        }

        public static IList<RaycastResult> Filter(
            this IList<RaycastResult> hits,
            int hitCount,
            CollisionFilter filter,
            out int resultHitCount)
        {
            hitCount = Math.Min(hitCount, hits.Count);
            resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var currentHit = hits[i];
                if (currentHit.distance <= filter.maxDistance) continue;

                int lastValidHitIndex = --resultHitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = currentHit;
            }

            return hits;
        }

        public static IList<RaycastHit> Filter(
            this IList<RaycastHit> hits,
            int hitCount,
            CollisionFilter filter,
            out int resultHitCount)
        {
            hitCount = Math.Min(hitCount, hits.Count);
            resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var currentHit = hits[i];
                if (currentHit.distance <= filter.maxDistance) continue;

                int lastValidHitIndex = --resultHitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = currentHit;
            }

            return hits;
        }

        public static bool TryGetMinimumDistanceHit(
            this IList<RaycastHit> hits,
            int hitCount,
            out RaycastHit hit)
        {
            hit = default;

            hitCount = Math.Min(hitCount, hits.Count);
            if (hitCount <= 0) return false;

            float minDistance = -1f;
            int hitIndex = -1;

            for (int i = 0; i < hitCount; i++) {
                var nextHit = hits[i];
                float distance = nextHit.distance;

                if (distance < minDistance) {
                    hitIndex = i;
                    minDistance = distance;
                    continue;
                }

                if (minDistance >= 0) continue;

                hitIndex = i;
                minDistance = distance;
            }

            if (hitIndex < 0) return false;

            hit = hits[hitIndex];
            return true;
        }

        public static bool TryGetMinimumDistanceHit(
            this IList<RaycastResult> hits,
            int hitCount,
            out RaycastResult hit)
        {
            hit = default;

            hitCount = Math.Min(hitCount, hits.Count);
            if (hitCount <= 0) return false;

            float minDistance = -1f;
            int hitIndex = -1;

            for (int i = 0; i < hitCount; i++) {
                var nextHit = hits[i];
                float distance = nextHit.distance;

                if (distance < minDistance) {
                    hitIndex = i;
                    minDistance = distance;
                    continue;
                }

                if (minDistance >= 0) continue;

                hitIndex = i;
                minDistance = distance;
            }

            if (hitIndex < 0) return false;

            hit = hits[hitIndex];
            return true;
        }

        public static string AsText(this IReadOnlyList<RaycastHit> hits, int hitCount) {
            var sb = new StringBuilder();

            sb.AppendLine($"Hits({hitCount})" + " {");

            hitCount = Math.Min(hitCount, hits.Count);
            for (int i = 0; i < hitCount; i++) {
                var hit = hits[i];
                bool hasContact = hit.transform != null;

                sb.AppendLine($" - Hit[{i}] : {(hasContact ? $"{hit.transform.name}::{hit.distance}" : "none")}");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string AsText(this IReadOnlyList<RaycastResult> hits, int hitCount) {
            var sb = new StringBuilder();

            sb.AppendLine($"Hits({hitCount})" + " {");

            hitCount = Math.Min(hitCount, hits.Count);
            for (int i = 0; i < hitCount; i++) {
                var hit = hits[i];
                bool hasContact = hit.gameObject != null;

                sb.AppendLine($" - Hit[{i}] : {(hasContact ? $"{hit.gameObject.name}::{hit.distance}" : "none")}");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }
    }
    
}
