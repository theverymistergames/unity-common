using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MisterGames.Collisions.Core;
using MisterGames.Common.Layers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.Collisions.Utils {
    
    public static class CollisionUtils {

        private static readonly IComparer<RaycastHit> RaycastHitDistanceComparerAsc = new RaycastHitDistanceComparer(true);
        private static readonly IComparer<RaycastHit> RaycastHitDistanceComparerDesc = new RaycastHitDistanceComparer(false);
        private static readonly IComparer<RaycastResult> RaycastResultDistanceComparerAsc = new RaycastResultDistanceComparer(true);
        private static readonly IComparer<RaycastResult> RaycastResultDistanceComparerDesc = new RaycastResultDistanceComparer(false);

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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponentFromRaycastHit<T>(this RaycastHit raycastHit) {
            return raycastHit.rigidbody is { } rb && rb.TryGetComponent(out T t)
                ? t
                : raycastHit.collider.GetComponent<T>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponentFromCollision<T>(this Collision collision) {
            return collision.rigidbody is { } rb && rb.TryGetComponent(out T t) 
                ? t 
                : collision.collider.GetComponent<T>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponentFromCollider<T>(this Collider collider) {
            return collider.attachedRigidbody is { } rb && rb.TryGetComponent(out T t) 
                ? t 
                : collider.GetComponent<T>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasLostContact(CollisionInfo lastInfo, CollisionInfo newInfo) {
            return lastInfo.hasContact && !newInfo.hasContact;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasNewContact(CollisionInfo lastInfo, CollisionInfo newInfo) {
            return !lastInfo.hasContact && newInfo.hasContact;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasContactWithTransform(this CollisionInfo info, Transform transform) {
            return info.hasContact && info.transform.GetHashCode() == transform.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTransformChanged(this CollisionInfo newInfo, CollisionInfo lastInfo) {
            return
                !lastInfo.hasContact && newInfo.hasContact ||
                lastInfo.hasContact && !newInfo.hasContact ||
                lastInfo.hasContact && lastInfo.transform.GetHashCode() != newInfo.transform.GetHashCode();
        }

        public static RaycastHit[] RemoveInvalidHits(
            this RaycastHit[] hits,
            ref int hitCount
        ) {
            hitCount = Math.Min(hitCount, hits.Length);
            
            for (int i = hitCount - 1; i >= 0; i--) {
                var currentHit = hits[i];
                if (currentHit.distance > 0f) continue;

                int lastValidHitIndex = --hitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = currentHit;
            }

            return hits;
        }

        public static Span<RaycastHit> RemoveInvalidHits(
            this Span<RaycastHit> hits,
            ref int hitCount
        ) {
            hitCount = Math.Min(hitCount, hits.Length);

            for (int i = hitCount - 1; i >= 0; i--) {
                var currentHit = hits[i];
                if (currentHit.distance > 0f) continue;

                int lastValidHitIndex = --hitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = currentHit;
            }

            return hits;
        }

        public static T RemoveInvalidHits<T>(
            this T hits,
            ref int hitCount
        ) where T : IList<RaycastResult> {
            hitCount = Math.Min(hitCount, hits.Count);

            for (int i = hitCount - 1; i >= 0; i--) {
                var currentHit = hits[i];
                if (currentHit.distance > 0f) continue;

                int lastValidHitIndex = --hitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = currentHit;
            }

            return hits;
        }
        
        public static RaycastHit[] IgnoreColliders(
            this RaycastHit[] hits,
            ref int hitCount,
            IReadOnlyList<Collider> ignoredColliders
        ) {
            hits.Filter(ref hitCount, ignoredColliders, (ignoreList, h) => {
                for (int i = 0; i < ignoreList.Count; i++) {
                    if (h.collider == ignoreList[i]) return false;
                }

                return true;
            });

            return hits;
        }
        
        public static Collider[] IgnoreColliders(
            this Collider[] colliders,
            ref int hitCount,
            IReadOnlyList<Collider> ignoredColliders
        ) {
            colliders.Filter(ref hitCount, ignoredColliders, (ignoreList, c) => {
                for (int i = 0; i < ignoreList.Count; i++) {
                    if (c == ignoreList[i]) return false;
                }

                return true;
            });

            return colliders;
        }
        
        public static RaycastHit[] Filter(
            this RaycastHit[] hits,
            ref int hitCount,
            Func<RaycastHit, bool> predicate
        ) {
            hitCount = Math.Min(hitCount, hits.Length);
            int resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var hit = hits[i];
                if (predicate.Invoke(hit)) continue;
                
                int j = --resultHitCount;
                hits[i] = hits[j];
                hits[j] = hit;
            }

            hitCount = resultHitCount;
            return hits;
        }

        public static RaycastHit[] Filter<T>(
            this RaycastHit[] hits,
            ref int hitCount,
            T data,
            Func<T, RaycastHit, bool> predicate
        ) {
            hitCount = Math.Min(hitCount, hits.Length);
            int resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var hit = hits[i];
                if (predicate.Invoke(data, hit)) continue;
                
                int j = --resultHitCount;
                hits[i] = hits[j];
                hits[j] = hit;
            }

            hitCount = resultHitCount;
            return hits;
        }
        
        public static Collider[] Filter(
            this Collider[] colliders,
            ref int hitCount,
            Func<Collider, bool> predicate
        ) {
            hitCount = Math.Min(hitCount, colliders.Length);
            int resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var hit = colliders[i];
                if (predicate.Invoke(hit)) continue;
                
                int j = --resultHitCount;
                colliders[i] = colliders[j];
                colliders[j] = hit;
            }

            hitCount = resultHitCount;
            return colliders;
        }
        
        public static Collider[] Filter<T>(
            this Collider[] colliders,
            ref int hitCount,
            T data,
            Func<T, Collider, bool> predicate
        ) {
            hitCount = Math.Min(hitCount, colliders.Length);
            int resultHitCount = hitCount;

            for (int i = hitCount - 1; i >= 0; i--) {
                var collider = colliders[i];
                if (predicate.Invoke(data, collider)) continue;
                
                int j = --resultHitCount;
                colliders[i] = colliders[j];
                colliders[j] = collider;
            }

            hitCount = resultHitCount;
            return colliders;
        }

        public static RaycastHit[] Filter(
            this RaycastHit[] hits,
            ref int hitCount,
            CollisionFilter filter
        ) {
            hitCount = Math.Min(hitCount, hits.Length);
            
            for (int i = hitCount - 1; i >= 0; i--) {
                var hit = hits[i];
                int layer = hit.transform.gameObject.layer;

                if (hit.distance <= filter.maxDistance &&
                    filter.layerMask.Contains(layer)
                ) {
                    continue;
                }

                int lastValidHitIndex = --hitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = hit;
            }

            return hits;
        }
        
        public static CollisionInfo[] Filter(
            this CollisionInfo[] hits,
            ref int hitCount,
            CollisionFilter filter
        ) {
            hitCount = Math.Min(hitCount, hits.Length);

            for (int i = hitCount - 1; i >= 0; i--) {
                var hit = hits[i];
                int layer = hit.transform.gameObject.layer;

                if (hit.distance <= filter.maxDistance &&
                    filter.layerMask.Contains(layer)
                ) {
                    continue;
                }

                int lastValidHitIndex = --hitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = hit;
            }

            return hits;
        }

        public static Span<RaycastHit> Filter(
            this Span<RaycastHit> hits,
            ref int hitCount,
            CollisionFilter filter
        ) {
            hitCount = Math.Min(hitCount, hits.Length);

            for (int i = hitCount - 1; i >= 0; i--) {
                var hit = hits[i];
                int layer = hit.transform.gameObject.layer;

                if (hit.distance <= filter.maxDistance &&
                    filter.layerMask.Contains(layer)
                ) {
                    continue;
                }

                int lastValidHitIndex = --hitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = hit;
            }

            return hits;
        }

        public static T Filter<T>(
            this T hits,
            ref int hitCount,
            CollisionFilter filter
        ) where T : IList<RaycastResult> {
            hitCount = Math.Min(hitCount, hits.Count);

            for (int i = hitCount - 1; i >= 0; i--) {
                var hit = hits[i];
                int layer = hit.gameObject.layer;

                if (hit.distance <= filter.maxDistance &&
                    filter.layerMask.Contains(layer)
                ) {
                    continue;
                }

                int lastValidHitIndex = --hitCount;
                hits[i] = hits[lastValidHitIndex];
                hits[lastValidHitIndex] = hit;
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
                if (distance <= 0f) continue;
                
                if (minDistance < 0f || distance < minDistance) {
                    hitIndex = i;
                    minDistance = distance;
                }
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
                if (distance <= 0f) continue;
                
                if (minDistance < 0f || distance < minDistance) {
                    hitIndex = i;
                    minDistance = distance;
                }
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
                if (distance <= 0f) continue;
                
                if (minDistance < 0f || distance < minDistance) {
                    hitIndex = i;
                    minDistance = distance;
                }
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
                if (distance <= 0f) continue;
                
                if (minDistance < 0 || distance < minDistance) {
                    hitIndex = i;
                    minDistance = distance;
                }
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
                if (distance <= 0f) continue;
                
                if (minDistance < 0f || distance < minDistance) {
                    hitIndex = i;
                    minDistance = distance;
                }
            }

            if (hitIndex < 0) return false;

            hit = hits[hitIndex];
            return true;
        }
    }
    
}
