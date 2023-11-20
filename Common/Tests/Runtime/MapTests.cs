using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using NUnit.Framework;
using Random = UnityEngine.Random;

namespace Data {

    public class MapTests {

        [Test]
        public void Contains() {
            var map = new Map<int, float>();

            map[0] = 1.0f;

            Assert.IsTrue(map.ContainsKey(0));
            Assert.IsTrue(map.IndexOf(0) >= 0);
            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(1f, map[0]);
            Assert.AreEqual(1f, map.Get(0));
            Assert.IsTrue(map.TryGetValue(0, out float value));
            Assert.AreEqual(1f, value);

            map.Remove(0);

            Assert.IsFalse(map.ContainsKey(0));
            Assert.IsFalse(map.IndexOf(0) >= 0);
            Assert.AreEqual(0, map.Count);

            Assert.Throws<KeyNotFoundException>(() => map.Get(0));
            Assert.IsFalse(map.TryGetValue(0, out value));
            Assert.AreEqual(0f, value);
        }

        [Test]
        public void Add() {
            var map = new Map<int, float>();

            map.Add(0, 1.0f);

            Assert.IsTrue(map.ContainsKey(0));
            Assert.IsTrue(map.IndexOf(0) >= 0);
            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(1f, map[0]);
            Assert.AreEqual(1f, map.Get(0));
            Assert.IsTrue(map.TryGetValue(0, out float value));
            Assert.AreEqual(1f, value);

            map.Add(1, 2.0f);

            Assert.IsTrue(map.ContainsKey(1));
            Assert.IsTrue(map.IndexOf(1) >= 0);
            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(2f, map[1]);
            Assert.AreEqual(2f, map.Get(1));
            Assert.IsTrue(map.TryGetValue(1, out value));
            Assert.AreEqual(2f, value);
        }

        [Test]
        public void AddDuplicate() {
            var map = new Map<int, float>();

            map.Add(0, 0.0f);
            Assert.Throws<ArgumentException>(() => { map.Add(0, 0.0f); });
        }

        [Test]
        public void SetDuplicate() {
            var map = new Map<int, float>();

            map[0] = 0.0f;
            map[0] = 1.0f;

            Assert.AreEqual(1f, map[0]);
            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void Clear() {
            var map = new Map<int, float>();

            map[0] = 1.0f;
            map[1] = 2.0f;

            map.Clear();
            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void GetValueByRef() {
            var map = new Map<int, float>();

            map[0] = 1.0f;
            ref float value = ref map.Get(0);

            Assert.AreEqual(1f, value);

            value = 2f;
            Assert.AreEqual(2f, value);
            Assert.AreEqual(2f, map[0]);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        public void AddRemoveRandom(int size) {
            var map = new Map<int, float>();
            var added = new Dictionary<int, float>();
            var removed = new HashSet<int>();

            const int iterations = 10;
            const float removePossibility = 0.33f;
            const int keys = 10;

            for (int i = 0; i < iterations; i++) {
                for (int j = 0; j < size; j++) {
                    int key = Random.Range(0, keys);
                    if (added.ContainsKey(key) || removed.Contains(key)) continue;

                    added[key] = key + 100f;
                    map[key] = key + 100f;
                }

                for (int j = 0; j < size; j++) {
                    if (Random.Range(0f, 1f) > removePossibility) continue;

                    int key = Random.Range(0, keys);
                    if (!added.ContainsKey(key) || removed.Contains(key)) continue;

                    removed.Add(key);
                    map.Remove(key);
                }

                foreach (int key in added.Keys) {
                    bool expectedContains = !removed.Contains(key);
                    bool actualContains = map.ContainsKey(key);

                    Assert.AreEqual(expectedContains, actualContains);
                }
            }
        }
    }

}
