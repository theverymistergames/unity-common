﻿using System.Collections.Generic;
using MisterGames.Blueprints.Core2;
using NUnit.Framework;
using UnityEngine;

namespace Core {

    public class BlueprintLinkStorageTests {

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(100)]
        public void AddLinks(int iterations) {
            var storage = new BlueprintLinkStorage();

            for (int i = 0; i < iterations; i++) {
                storage.AddLink(0L, 0, i, i);

                Assert.IsTrue(storage.TryGetLinksFrom(0L, 0, out int firstLink));
                var link = storage.GetLink(firstLink);

                Assert.AreEqual(i, link.nodeId);
                Assert.AreEqual(i, link.port);
            }
        }

        [Test]
        public void AddPortLinks() {
            var storage = new BlueprintLinkStorage();

            storage.AddLink(0L, 0, 0L, 0);
            storage.AddLink(0L, 0, 0L, 1);
            storage.AddLink(0L, 0, 0L, 2);

            Assert.IsTrue(storage.TryGetLinksFrom(0L, 0, out int link));
            Assert.AreEqual(2, storage.GetLink(link).port);

            Assert.IsTrue(storage.TryGetNextLink(link, out link));
            Assert.AreEqual(1, storage.GetLink(link).port);

            Assert.IsTrue(storage.TryGetNextLink(link, out link));
            Assert.AreEqual(0, storage.GetLink(link).port);

            Assert.IsFalse(storage.TryGetNextLink(link, out link));
        }

        [Test]
        public void AddNodeLinks() {
            var storage = new BlueprintLinkStorage();

            storage.AddLink(0L, 0, 0L, 1);
            storage.AddLink(0L, 1, 0L, 2);
            storage.AddLink(0L, 2, 0L, 3);

            Assert.IsTrue(storage.TryGetLinksFrom(0L, 0, out int link));
            Assert.AreEqual(1, storage.GetLink(link).port);

            Assert.IsTrue(storage.TryGetLinksFrom(0L, 1, out link));
            Assert.AreEqual(2, storage.GetLink(link).port);

            Assert.IsTrue(storage.TryGetLinksFrom(0L, 2, out link));
            Assert.AreEqual(3, storage.GetLink(link).port);
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

            Assert.IsFalse(storage.ContainsLink(0L, 0, 0L, 0));
            Assert.IsFalse(storage.ContainsLink(0L, 1, 0L, 0));
            Assert.IsFalse(storage.ContainsLink(0L, 2, 0L, 0));
        }

        [Test]
        public void RemovePortLinks() {
            var storage = new BlueprintLinkStorage();

            storage.AddLink(0L, 0, 0L, 0);
            storage.AddLink(0L, 0, 0L, 1);

            storage.RemovePort(0L, 0);

            Assert.IsFalse(storage.ContainsLink(0L, 0, 0L, 0));
            Assert.IsFalse(storage.ContainsLink(0L, 0, 0L, 1));
        }

        [Test]
        public void RemoveNodeLinks() {
            var storage = new BlueprintLinkStorage();

            storage.AddLink(0L, 0, 0L, 0);
            storage.AddLink(0L, 1, 0L, 0);

            storage.RemoveNode(0L);

            Assert.IsFalse(storage.ContainsLink(0L, 0, 0L, 0));
            Assert.IsFalse(storage.ContainsLink(0L, 1, 0L, 0));
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
                    Assert.IsTrue(storage.ContainsLink(fromNodeId, fromPort, toNodeId, toPort));
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
                    bool hasLinkActual = storage.ContainsLink(fromNodeId, fromPort, toNodeId, toPort);

                    Assert.AreEqual(hasLinkExpected, hasLinkActual);
                }
            }
        }
    }

}
