using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CollisionUtilsTests {

    public class CollisionUtilsTests {

        [Test]
        public void RemoveInvalidHits_RaycastHit() {
            int hitCount = 100;
            var hits = new RaycastHit[hitCount];

            for (int i = 0; i < hitCount; i++) {
                hits[i] = new RaycastHit { distance = Random.Range(-1f, 1f) };
            }

            hits.RemoveInvalidHits(ref hitCount);

            for (int i = 0; i < hitCount; i++) {
                Assert.IsTrue(hits[i].distance > 0f);
            }
        }

        [Test]
        public void RemoveInvalidHits_RaycastResult() {
            int hitCount = 100;
            var hits = new List<RaycastResult>(hitCount);

            for (int i = 0; i < hitCount; i++) {
                hits.Add(new RaycastResult { distance = Random.Range(-1f, 1f) });
            }

            hits.RemoveInvalidHits(ref hitCount);

            for (int i = 0; i < hitCount; i++) {
                Assert.IsTrue(hits[i].distance > 0f);
            }
        }


        [Test]
        public void Filter_RaycastHit() {
            int hitCount = 100;
            var filter = new CollisionFilter { maxDistance = 0.5f };

            var hits = new RaycastHit[hitCount];

            for (int i = 0; i < hitCount; i++) {
                hits[i] = new RaycastHit { distance = Random.Range(0f, 1f) };
            }

            hits.Filter(ref hitCount, filter);

            for (int i = 0; i < hitCount; i++) {
                Assert.IsTrue(hits[i].distance <= 0.5f);
            }
        }

        [Test]
        public void Filter_RaycastResult() {
            int hitCount = 100;
            var filter = new CollisionFilter { maxDistance = 0.5f };

            var hits = new List<RaycastResult>(hitCount);

            for (int i = 0; i < hitCount; i++) {
                hits.Add(new RaycastResult { distance = Random.Range(0f, 1f) });
            }

            hits.Filter(ref hitCount, filter);

            for (int i = 0; i < hitCount; i++) {
                Assert.IsTrue(hits[i].distance <= 0.5f);
            }
        }

        [Test]
        public void SortByDistanceAsc_RaycastHit() {
            int hitCount = 100;
            var hits = new RaycastHit[hitCount];

            for (int i = 0; i < hitCount; i++) {
                hits[i] = new RaycastHit { distance = Random.Range(0f, 1f) };
            }

            hits.SortByDistance(hitCount, ascending: true);

            for (int i = 1; i < hitCount; i++) {
                Assert.IsTrue(hits[i].distance >= hits[i - 1].distance);
            }
        }

        [Test]
        public void SortByDistanceDesc_RaycastHit() {
            int hitCount = 100;
            var hits = new RaycastHit[hitCount];

            for (int i = 0; i < hitCount; i++) {
                hits[i] = new RaycastHit { distance = Random.Range(0f, 1f) };
            }

            hits.SortByDistance(hitCount, ascending: false);

            for (int i = 1; i < hitCount; i++) {
                Assert.IsTrue(hits[i].distance <= hits[i - 1].distance);
            }
        }

        [Test]
        public void SortByDistanceAsc_RaycastResult() {
            int hitCount = 100;
            var hits = new List<RaycastResult>(hitCount);

            for (int i = 0; i < hitCount; i++) {
                hits.Add(new RaycastResult { distance = Random.Range(0f, 1f) });
            }

            hits.SortByDistance(hitCount, ascending: true);

            for (int i = 1; i < hitCount; i++) {
                Assert.IsTrue(hits[i].distance >= hits[i - 1].distance);
            }
        }

        [Test]
        public void SortByDistanceDesc_RaycastResult() {
            int hitCount = 100;
            var hits = new List<RaycastResult>(hitCount);

            for (int i = 0; i < hitCount; i++) {
                hits.Add(new RaycastResult { distance = Random.Range(0f, 1f) });
            }

            hits.SortByDistance(hitCount, ascending: false);

            for (int i = 1; i < hitCount; i++) {
                Assert.IsTrue(hits[i].distance <= hits[i - 1].distance);
            }
        }

        [Test]
        public void TryGetMinimumDistanceHit_RaycastHit() {
            int hitCount = 100;
            var hits = new RaycastHit[hitCount];

            float min = 2f;
            for (int i = 0; i < hitCount; i++) {
                float value = Random.Range(1f, 2f);
                hits[i] = new RaycastHit { distance = value };

                if (value < min) min = value;
            }

            bool hasMinimumDistanceHit = hits.TryGetMinimumDistanceHit(hitCount, out var hit);
            Assert.IsTrue(hasMinimumDistanceHit);
            Assert.AreEqual(min, hit.distance);
        }

        [Test]
        public void TryGetMinimumDistanceHit_RaycastResult() {
            int hitCount = 100;
            var hits = new List<RaycastResult>(hitCount);

            float min = 2f;
            for (int i = 0; i < hitCount; i++) {
                float value = Random.Range(1f, 2f);
                hits.Add(new RaycastResult { distance = value });

                if (value < min) min = value;
            }

            bool hasMinimumDistanceHit = hits.TryGetMinimumDistanceHit(hitCount, out var hit);
            Assert.IsTrue(hasMinimumDistanceHit);
            Assert.AreEqual(min, hit.distance);
        }

        [Test]
        public void TryGetMinimumDistanceHit_RaycastHit_ReturnsFalse_IfHitCount_IsZero() {
            int hitCount = 100;
            var hits = new RaycastHit[hitCount];

            for (int i = 0; i < hitCount; i++) {
                hits[i] = new RaycastHit { distance = Random.Range(1f, 2f) };
            }

            bool hasMinimumDistanceHit = hits.TryGetMinimumDistanceHit(0, out var hit);
            Assert.IsFalse(hasMinimumDistanceHit);
        }

        [Test]
        public void TryGetMinimumDistanceHit_RaycastResult_ReturnsFalse_IfHitCount_IsZero() {
            int hitCount = 100;
            var hits = new List<RaycastResult>(hitCount);

            for (int i = 0; i < hitCount; i++) {
                hits.Add(new RaycastResult { distance = Random.Range(1f, 2f) });
            }

            bool hasMinimumDistanceHit = hits.TryGetMinimumDistanceHit(0, out var hit);
            Assert.IsFalse(hasMinimumDistanceHit);
        }
    }

}
