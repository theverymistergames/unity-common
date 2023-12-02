using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Core {

    public class BlueprintLinkStorageTests {

        private class LinksComparer : IComparer<BlueprintLink> {
            public int Compare(BlueprintLink x, BlueprintLink y) {
                return x.id == y.id ? x.port.CompareTo(y.port) : x.id.node.CompareTo(y.id.node);
            }
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        [TestCase(100)]
        public void AddLinks(int iterations) {
            var storage = new BlueprintLinkStorage();
            var id = new NodeId(0, 0);

            for (int i = 0; i < iterations; i++) {
                var toId = new NodeId(0, i);
                storage.AddLink(id, 0, toId, i);

                Assert.IsTrue(storage.TryGetLinksFrom(id, 0, out int firstLink));
                var link = storage.GetLink(firstLink);

                Assert.AreEqual(i, link.id.node);
                Assert.AreEqual(i, link.port);
            }

            Assert.AreEqual(1, storage.LinkedPortCount);
            Assert.AreEqual(iterations, storage.LinkCount);
        }

        [Test]
        public void AddPortLinks() {
            var storage = new BlueprintLinkStorage();
            var id = new NodeId(0, 0);

            storage.AddLink(id, 0, id, 0);
            storage.AddLink(id, 0, id, 1);
            storage.AddLink(id, 0, id, 2);

            Assert.AreEqual(3, storage.LinkCount);
            Assert.AreEqual(1, storage.LinkedPortCount);

            Assert.IsTrue(storage.TryGetLinksFrom(id, 0, out int link));
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
            var id = new NodeId(0, 0);

            storage.AddLink(id, 0, id, 1);
            storage.AddLink(id, 1, id, 2);
            storage.AddLink(id, 2, id, 3);

            Assert.AreEqual(3, storage.LinkCount);
            Assert.AreEqual(3, storage.LinkedPortCount);

            Assert.IsTrue(storage.TryGetLinksFrom(id, 0, out int link));
            Assert.AreEqual(1, storage.GetLink(link).port);

            Assert.IsTrue(storage.TryGetLinksFrom(id, 1, out link));
            Assert.AreEqual(2, storage.GetLink(link).port);

            Assert.IsTrue(storage.TryGetLinksFrom(id, 2, out link));
            Assert.AreEqual(3, storage.GetLink(link).port);
        }

        [Test]
        public void RemoveLinks() {
            var storage = new BlueprintLinkStorage();
            var id = new NodeId(0, 0);

            storage.AddLink(id, 0, id, 0);
            storage.AddLink(id, 2, id, 0);
            storage.AddLink(id, 1, id, 0);

            Assert.AreEqual(3, storage.LinkCount);
            Assert.AreEqual(3, storage.LinkedPortCount);

            storage.RemoveLink(id, 0, id, 0);
            storage.RemoveLink(id, 1, id, 0);
            storage.RemoveLink(id, 2, id, 0);

            Assert.AreEqual(0, storage.LinkCount);
            Assert.AreEqual(0, storage.LinkedPortCount);

            Assert.IsFalse(storage.ContainsLink(id, 0, id, 0));
            Assert.IsFalse(storage.ContainsLink(id, 1, id, 0));
            Assert.IsFalse(storage.ContainsLink(id, 2, id, 0));
        }

        [Test]
        public void RemovePortLinks() {
            var storage = new BlueprintLinkStorage();
            var id = new NodeId(0, 0);

            storage.AddLink(id, 0, id, 0);
            storage.AddLink(id, 0, id, 1);

            Assert.AreEqual(2, storage.LinkCount);
            Assert.AreEqual(1, storage.LinkedPortCount);

            storage.RemovePort(id, 0);

            Assert.AreEqual(0, storage.LinkCount);
            Assert.AreEqual(0, storage.LinkedPortCount);

            Assert.IsFalse(storage.ContainsLink(id, 0, id, 0));
            Assert.IsFalse(storage.ContainsLink(id, 0, id, 1));
        }

        [Test]
        public void RemoveNodeLinks() {
            var storage = new BlueprintLinkStorage();
            var id = new NodeId(0, 0);

            storage.AddLink(id, 0, id, 0);
            storage.AddLink(id, 1, id, 0);

            Assert.AreEqual(2, storage.LinkCount);
            Assert.AreEqual(2, storage.LinkedPortCount);

            storage.RemoveNode(id);

            Assert.AreEqual(0, storage.LinkCount);
            Assert.AreEqual(0, storage.LinkedPortCount);

            Assert.IsFalse(storage.ContainsLink(id, 0, id, 0));
            Assert.IsFalse(storage.ContainsLink(id, 1, id, 0));
        }

        [Test]
        public void SortLinks() {
            var storage = new BlueprintLinkStorage();
            var id = new NodeId(0, 0);

            storage.AddLink(id, 0, id, 0);
            storage.AddLink(id, 0, id, 1);
            storage.AddLink(id, 0, id, 2);

            storage.SortLinksFrom(id, 0, new LinksComparer());

            storage.TryGetLinksFrom(id, 0, out int l);

            var link = storage.GetLink(l);
            Assert.AreEqual(0, link.port);

            storage.TryGetNextLink(l, out l);
            link = storage.GetLink(l);
            Assert.AreEqual(1, link.port);

            storage.TryGetNextLink(l, out l);
            link = storage.GetLink(l);
            Assert.AreEqual(2, link.port);
        }

        [Test]
        public void CopyLinks() {
            var storage = new BlueprintLinkStorage();

            var id0 = new NodeId(0, 0);
            var id1 = new NodeId(0, 1);

            storage.AddLink(id0, 0, id1, 0);
            storage.AddLink(id0, 1, id1, 1);

            var links = storage.CopyLinks(id0);

            var tree = links.GetTree(new BlueprintLink(id0, 0));
            Assert.IsTrue(tree.MoveChild(new BlueprintLink(id0, 0)));
            Assert.IsTrue(tree.MoveChild());

            Assert.AreEqual(id1, tree.GetKey().id);
            Assert.AreEqual(0, tree.GetKey().port);

            tree = links.GetTree(new BlueprintLink(id0, 1));
            Assert.IsTrue(tree.MoveChild(new BlueprintLink(id0, 0)));
            Assert.IsTrue(tree.MoveChild());

            Assert.AreEqual(id1, tree.GetKey().id);
            Assert.AreEqual(1, tree.GetKey().port);
        }

        [Test]
        public void SetLinks() {
            var storage = new BlueprintLinkStorage();

            var id0 = new NodeId(0, 0);
            var id1 = new NodeId(0, 1);

            storage.AddLink(id0, 0, id1, 0);
            storage.AddLink(id0, 1, id1, 1);

            var links = storage.CopyLinks(id0);

            storage = new BlueprintLinkStorage();

            storage.SetLinks(id0, 0, links, 0);
            storage.SetLinks(id0, 1, links, 1);

            Assert.IsTrue(storage.ContainsLink(id0, 0, id1, 0));
            Assert.IsTrue(storage.ContainsLink(id0, 1, id1, 1));
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
            var addedLinks = new HashSet<(NodeId, int, NodeId, int)>();

            const int size = 10;
            const int maxSources = 4;
            const int maxNodes = 4;
            const int maxPorts = 5;

            for (int i = 0; i < iterations; i++) {
                for (int j = 0; j < size; j++) {
                    var fromNodeId = new NodeId(Random.Range(1, maxSources), Random.Range(1, maxNodes));
                    int fromPort = Random.Range(0, maxPorts - 1);

                    var toNodeId = new NodeId(Random.Range(1, maxSources), Random.Range(1, maxNodes));
                    int toPort = Random.Range(0, maxPorts - 1);

                    if (addedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort))) continue;

                    storage.AddLink(fromNodeId, fromPort, toNodeId, toPort);
                    addedLinks.Add((fromNodeId, fromPort, toNodeId, toPort));
                }

                foreach ((var fromNodeId, int fromPort, var toNodeId, int toPort) in addedLinks) {
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
            var addedLinks = new HashSet<(NodeId, int, NodeId, int)>();
            var removedLinks = new HashSet<(NodeId, int, NodeId, int)>();
            var linkedPorts = new Dictionary<(NodeId, int), int>();

            const int size = 10;
            const int maxSources = 4;
            const int maxNodes = 4;
            const int maxPorts = 5;
            const float removePossibility = 0.33f;

            for (int i = 0; i < iterations; i++) {
                for (int j = 0; j < size; j++) {
                    var fromNodeId = new NodeId(Random.Range(1, maxSources), Random.Range(1, maxNodes));
                    int fromPort = Random.Range(0, maxPorts - 1);

                    var toNodeId = new NodeId(Random.Range(1, maxSources), Random.Range(1, maxNodes));
                    int toPort = Random.Range(0, maxPorts - 1);

                    if (addedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort)) ||
                        removedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort))) continue;

                    storage.AddLink(fromNodeId, fromPort, toNodeId, toPort);
                    addedLinks.Add((fromNodeId, fromPort, toNodeId, toPort));

                    if (linkedPorts.TryGetValue((fromNodeId, fromPort), out int links)) {
                        linkedPorts[(fromNodeId, fromPort)] = links + 1;
                    }
                    else {
                        linkedPorts[(fromNodeId, fromPort)] = 1;
                    }
                }

                for (int j = 0; j < size; j++) {
                    if (Random.Range(0f, 1f) > removePossibility) continue;

                    var fromNodeId = new NodeId(Random.Range(1, maxSources), Random.Range(1, maxNodes));
                    int fromPort = Random.Range(0, maxPorts - 1);

                    var toNodeId = new NodeId(Random.Range(1, maxSources), Random.Range(1, maxNodes));
                    int toPort = Random.Range(0, maxPorts - 1);

                    if (!addedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort)) ||
                        removedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort))) continue;

                    storage.RemoveLink(fromNodeId, fromPort, toNodeId, toPort);
                    removedLinks.Add((fromNodeId, fromPort, toNodeId, toPort));

                    linkedPorts[(fromNodeId, fromPort)]--;
                    if (linkedPorts[(fromNodeId, fromPort)] <= 0) linkedPorts.Remove((fromNodeId, fromPort));
                }

                foreach ((var fromNodeId, int fromPort, var toNodeId, int toPort) in addedLinks) {
                    bool hasLinkExpected = !removedLinks.Contains((fromNodeId, fromPort, toNodeId, toPort));
                    bool hasLinkActual = storage.ContainsLink(fromNodeId, fromPort, toNodeId, toPort);

                    Assert.AreEqual(hasLinkExpected, hasLinkActual);
                }

                Assert.AreEqual(addedLinks.Count - removedLinks.Count, storage.LinkCount);
                Assert.AreEqual(linkedPorts.Count, storage.LinkedPortCount);
            }
        }
    }

}
