using System.Collections.Generic;
using MisterGames.Blueprints.Core2;
using NUnit.Framework;
using UnityEngine;

namespace Core {
/*
    public class BlueprintLinkStorageTests {

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(100)]
        public void AddLinks(int iterations) {
            var storage = new BlueprintLinkStorage();

            for (int i = 0; i < iterations; i++) {
                bool added = storage.AddLink(0L, 0, i, i);
                Assert.IsTrue(added);

                storage.TryGetFirstLink(0L, 0, out int index, out int count);
                Assert.AreEqual(i + 1, count);

                var link = storage.GetLink(index + i);
                Assert.AreEqual(i, link.nodeId);
                Assert.AreEqual(i, link.port);
            }
        }

        [Test]
        public void AddDuplicatedLinks() {
            var storage = new BlueprintLinkStorage();

            storage.AddLink(0L, 0, 0L, 0);

            bool added = storage.AddLink(0L, 0, 0L, 0);
            Assert.IsFalse(added);

            added = storage.AddLink(0L, 0, 0L, 0);
            Assert.IsFalse(added);

            storage.TryGetFirstLink(0L, 0, out int index, out int count);

            Assert.AreEqual(1, count);
        }

        [Test]
        public void AddPortLinks() {
            var storage = new BlueprintLinkStorage();

            storage.AddLink(0L, 0, 0L, 0);
            storage.AddLink(0L, 0, 0L, 1);
            storage.AddLink(0L, 0, 0L, 2);

            storage.TryGetFirstLink(0L, 0, out int index, out int count);

            Assert.AreEqual(3, count);

            Assert.IsTrue(storage.HasLink(0L, 0, 0L, 0));
            Assert.IsTrue(storage.HasLink(0L, 0, 0L, 1));
            Assert.IsTrue(storage.HasLink(0L, 0, 0L, 2));
        }

        [Test]
        public void AddNodeLinks() {
            var storage = new BlueprintLinkStorage();

            storage.AddLink(0L, 1, 0L, 0);
            storage.AddLink(0L, 0, 0L, 0);

            Assert.IsTrue(storage.HasLink(0L, 0, 0L, 0));
            Assert.IsTrue(storage.HasLink(0L, 1, 0L, 0));
        }

        [Test]
        public void RemoveLinks() {
            var storage = new BlueprintLinkStorage();

            storage.AddLink(0L, 0, 0L, 0);
            storage.AddLink(0L, 2, 0L, 0);
            storage.AddLink(0L, 1, 0L, 0);

            storage.RemoveLink(0L, 0, 0L, 0);
            storage.RemoveLink(0L, 1, 0L, 0);
            storage.RemoveLink(0L, 2, 0L, 0);

            Assert.IsFalse(storage.HasLink(0L, 0, 0L, 0));
            Assert.IsFalse(storage.HasLink(0L, 1, 0L, 0));
            Assert.IsFalse(storage.HasLink(0L, 2, 0L, 0));
        }

        [Test]
        public void RemovePortLinks() {
            var storage = new BlueprintLinkStorage();

            storage.AddLink(0L, 0, 0L, 0);
            storage.AddLink(0L, 0, 0L, 1);

            storage.RemovePortLinks(0L, 0);

            Assert.IsFalse(storage.HasLink(0L, 0, 0L, 0));
            Assert.IsFalse(storage.HasLink(0L, 0, 0L, 1));
        }

        [Test]
        public void RemoveNodeLinks() {
            var storage = new BlueprintLinkStorage();

            storage.AddLink(0L, 0, 0L, 0);
            storage.AddLink(0L, 1, 0L, 0);

            storage.RemoveNodeLinks(0L);

            Assert.IsFalse(storage.HasLink(0L, 0, 0L, 0));
            Assert.IsFalse(storage.HasLink(0L, 1, 0L, 0));
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(100)]
        public void AddRandomLinks(int iterations) {
            var storage = new BlueprintLinkStorage();
            var addedLinks = new HashSet<(long, int, long, int)>();

            const int size = 10;
            const int maxNodes = 3;
            const int maxPorts = 3;

            for (int i = 0; i < iterations; i++) {
                for (int j = 0; j < size; j++) {
                    long fromNodeId = Random.Range(1, maxNodes);
                    int fromPort = Random.Range(0, maxPorts - 1);

                    long toNodeId = Random.Range(1, maxNodes);
                    int toPort = Random.Range(0, maxPorts - 1);

                    if (addedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort))) continue;

                    storage.AddLink(fromNodeId, fromPort, toNodeId, toPort);
                    addedLinks.Add((fromNodeId, fromPort, toNodeId, toPort));
                }

                foreach ((long fromNodeId, int fromPort, long toNodeId, int toPort) in addedLinks) {
                    Assert.IsTrue(storage.HasLink(fromNodeId, fromPort, toNodeId, toPort));
                }
            }
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(100)]
        public void AddRemoveRandomLinks(int iterations) {
            var storage = new BlueprintLinkStorage();
            var addedLinks = new HashSet<(long, int, long, int)>();
            var removedLinks = new HashSet<(long, int, long, int)>();

            const int size = 10;
            const int maxNodes = 3;
            const int maxPorts = 3;
            const float removePossibility = 0.33f;

            for (int i = 0; i < iterations; i++) {
                for (int j = 0; j < size; j++) {
                    long fromNodeId = Random.Range(1, maxNodes);
                    int fromPort = Random.Range(0, maxPorts - 1);

                    long toNodeId = Random.Range(1, maxNodes);
                    int toPort = Random.Range(0, maxPorts - 1);

                    if (addedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort)) ||
                        removedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort))) continue;

                    storage.AddLink(fromNodeId, fromPort, toNodeId, toPort);
                    addedLinks.Add((fromNodeId, fromPort, toNodeId, toPort));
                }

                for (int j = 0; j < size; j++) {
                    if (Random.Range(0f, 1f) > removePossibility) continue;

                    long fromNodeId = Random.Range(1, maxNodes);
                    int fromPort = Random.Range(0, maxPorts - 1);

                    long toNodeId = Random.Range(1, maxNodes);
                    int toPort = Random.Range(0, maxPorts - 1);

                    if (removedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort))) continue;

                    storage.RemoveLink(fromNodeId, fromPort, toNodeId, toPort);
                    removedLinks.Add((fromNodeId, fromPort, toNodeId, toPort));
                }

                foreach ((long fromNodeId, int fromPort, long toNodeId, int toPort) in addedLinks) {
                    bool hasLinkExpected = !removedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort));
                    bool hasLinkActual = storage.HasLink(fromNodeId, fromPort, toNodeId, toPort);

                    Assert.AreEqual(hasLinkExpected, hasLinkActual);
                }
            }
        }
    }
*/
}
