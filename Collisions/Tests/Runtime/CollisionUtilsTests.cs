using MisterGames.Collisions.Utils;
using NUnit.Framework;
using UnityEngine;

namespace CollisionUtilsTests {

    public class CollisionUtilsTests {

        [Test]
        public void RemoveInvalidHits_RaycastHit() {
            int hitCount = 100;
            var hits = new RaycastHit[hitCount];

            for (int i = 0; i < hitCount; i++) {
                hits[i].distance = Random.Range(-1f, 1f);
            }

            hits.RemoveInvalidHits(hitCount, out hitCount);

            for (int i = 0; i < hitCount; i++) {
                Assert.IsTrue(hits[i].distance > 0f);
            }
        }

    }

}
