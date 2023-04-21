using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Collisions.Core;
using MisterGames.Common.Layers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.Collisions.Utils {
    
    public static class CollisionUtils {

        private static readonly IComparer<RaycastHit> RaycastHitDistanceComparerAsc
            = new RaycastHitDistanceComparer(true);

        private static readonly IComparer<RaycastHit> RaycastHitDistanceComparerDesc
            = new RaycastHitDistanceComparer(false);

        private static readonly IComparer<RaycastResult> RaycastResultDistanceComparerAsc
            = new RaycastResultDistanceComparer(true);

        private static readonly IComparer<RaycastResult> RaycastResultDistanceComparerDesc
            = new RaycastResultDistanceComparer(false);

        private sealed class RaycastHitDistanceComparer : IComparer<RaycastHit> {
            private readonly int _orderSign;
            public RaycastHitDistanceComparer(bool ascending) => _orderSign = ascending ? 1 : -1;
            public int Compare(RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance) * _orderSign;
        }

        private sealed class RaycastResultDistanceComparer : IComparer<RaycastResult> {
            private readonly int _orderSign;
            public RaycastResultDistanceComparer(bool ascending) => _orderSign = ascending ? 1 : -1;
            public int Compare(RaycastResult x, RaycastResult y) => x.distance.CompareTo(y.distance) * _orderSign;
        }

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

        public static RaycastHit[] RemoveInvalidHits(
            this RaycastHit[] hits,
            int hitCount,
            out int resultHitCount
        ) {
            hitCount = Math.Min(hitCount, hits.Length);
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

        public static Span<RaycastHit> RemoveInvalidHits(
            this Span<RaycastHit> hits,
            int hitCount,
            out int resultHitCount
        ) {
            hitCount = Math.Min(hitCount, hits.Length);
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

        public static T RemoveInvalidHits<T>(
            this T hits,
            int hitCount,
            out int resultHitCount
        ) where T : IList<RaycastResult> {
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

        public static RaycastHit[] Filter(
            this RaycastHit[] hits,
            int hitCount,
            CollisionFilter filter,
            out int resultHitCount
        ) {
            hitCount = Math.Min(hitCount, hits.Length);
            resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var currentHit = hits[i];
                if (currentHit.distance <= filter.maxDistance) continue;
                if (!filter.layerMask.Contains(currentHit.transform.gameObject.layer)) continue;

                int lastValidHitIndex = --resultHitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = currentHit;
            }

            return hits;
        }

        public static Span<RaycastHit> Filter(
            this Span<RaycastHit> hits,
            int hitCount,
            CollisionFilter filter,
            out int resultHitCount
        ) {
            hitCount = Math.Min(hitCount, hits.Length);
            resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var currentHit = hits[i];
                if (currentHit.distance <= filter.maxDistance) continue;
                if (!filter.layerMask.Contains(currentHit.transform.gameObject.layer)) continue;

                int lastValidHitIndex = --resultHitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = currentHit;
            }

            return hits;
        }

        public static T Filter<T>(
            this T hits,
            int hitCount,
            CollisionFilter filter,
            out int resultHitCount
        ) where T : IList<RaycastResult> {
            hitCount = Math.Min(hitCount, hits.Count);
            resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var currentHit = hits[i];
                if (currentHit.distance <= filter.maxDistance) continue;
                if (!filter.layerMask.Contains(currentHit.gameObject.layer)) continue;

                int lastValidHitIndex = --resultHitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = currentHit;
            }

            return hits;
        }

        public static RaycastHit[] SortByDistance(
            this RaycastHit[] hits,
            int hitCount,
            bool ascending = true
        ) {
            hitCount = Math.Min(hitCount, hits.Length);

            var comparer = ascending ? RaycastHitDistanceComparerAsc : RaycastHitDistanceComparerDesc;
            Array.Sort(hits, 0, hitCount, comparer);

            return hits;
        }

        public static List<RaycastResult> SortByDistance(
            this List<RaycastResult> hits,
            int hitCount,
            bool ascending = true
        ) {
            hitCount = Math.Min(hitCount, hits.Count);

            var comparer = ascending ? RaycastResultDistanceComparerAsc : RaycastResultDistanceComparerDesc;
            hits.Sort(0, hitCount, comparer);

            return hits;
        }

        public static bool TryGetMinimumDistanceHit(
            this ReadOnlySpan<CollisionInfo> hits,
            int hitCount,
            out CollisionInfo hit
        ) {
            hit = default;

            hitCount = Math.Min(hitCount, hits.Length);
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
            this IReadOnlyList<CollisionInfo> hits,
            int hitCount,
            out CollisionInfo hit
        ) {
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
            this ReadOnlySpan<RaycastHit> hits,
            int hitCount,
            out RaycastHit hit
        ) {
            hit = default;

            hitCount = Math.Min(hitCount, hits.Length);
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
            this IReadOnlyList<RaycastHit> hits,
            int hitCount,
            out RaycastHit hit
        ) {
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
            this IReadOnlyList<RaycastResult> hits,
            int hitCount,
            out RaycastResult hit
        ) {
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

        public static string AsText(this ReadOnlySpan<RaycastHit> hits, int hitCount) {
            var sb = new StringBuilder();

            sb.AppendLine($"Hits({hitCount})" + " {");

            hitCount = Math.Min(hitCount, hits.Length);
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
