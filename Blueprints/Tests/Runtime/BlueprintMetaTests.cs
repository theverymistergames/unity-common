using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core2;
using NUnit.Framework;
using UnityEngine;

namespace Core {

    public class BlueprintMetaTests {

        [Test]
        public void AddNode() {
            var meta = new BlueprintMeta2();
            var changed = new HashSet<long>();

            meta.Bind(id => changed.Add(id));
            long id = meta.AddNode(typeof(BlueprintSourceTest0));

            Assert.IsTrue(changed.Contains(id));
            Assert.IsTrue(meta.ContainsNode(id));

            Assert.AreEqual(2, meta.GetPortCount(id));
            Assert.IsTrue(meta.GetPort(id, 0).IsInput());
            Assert.IsFalse(meta.GetPort(id, 1).IsInput());
        }

        [Test]
        public void RemoveNode() {
            var meta = new BlueprintMeta2();
            var changed = new HashSet<long>();

            long id0 = meta.AddNode(typeof(BlueprintSourceTest0));
            long id1 = meta.AddNode(typeof(BlueprintSourceTest0));
            long id2 = meta.AddNode(typeof(BlueprintSourceTest0));

            meta.TryCreateLink(id0, 1, id1, 0);
            meta.TryCreateLink(id0, 1, id2, 0);

            meta.Bind(id => changed.Add(id));
            meta.RemoveNode(id0);

            Assert.AreEqual(0, meta.GetPortCount(id0));

            Assert.IsTrue(changed.Contains(id0));
            Assert.IsTrue(changed.Contains(id1));
            Assert.IsTrue(changed.Contains(id2));

            Assert.IsFalse(meta.ContainsNode(id0));
            Assert.IsFalse(meta.TryGetLinksFrom(id0, 1, out _));
            Assert.IsFalse(meta.TryGetLinksTo(id1, 0, out _));
            Assert.IsFalse(meta.TryGetLinksTo(id2, 0, out _));
        }

        [Test]
        public void AddLink() {
            var meta = new BlueprintMeta2();
            var changed = new HashSet<long>();

            long id0 = meta.AddNode(typeof(BlueprintSourceTest0));
            long id1 = meta.AddNode(typeof(BlueprintSourceTest1));

            meta.Bind(id => changed.Add(id));
            Assert.IsTrue(meta.TryCreateLink(id0, 1, id1, 0));

            Assert.IsTrue(changed.Contains(id0));
            Assert.IsTrue(changed.Contains(id1));

            Assert.IsTrue(meta.TryGetLinksFrom(id0, 1, out int index));

            var link = meta.GetLink(index);
            Assert.AreEqual(id1, link.nodeId);
            Assert.AreEqual(0, link.port);

            Assert.IsTrue(meta.TryGetLinksTo(id1, 0, out index));

            link = meta.GetLink(index);
            Assert.AreEqual(id0, link.nodeId);
            Assert.AreEqual(1, link.port);
        }

        [Test]
        public void RemoveLink() {
            var meta = new BlueprintMeta2();
            var changed = new HashSet<long>();

            long id0 = meta.AddNode(typeof(BlueprintSourceTest0));
            long id1 = meta.AddNode(typeof(BlueprintSourceTest1));

            meta.TryCreateLink(id0, 1, id1, 0);

            meta.Bind(id => changed.Add(id));
            meta.RemoveLink(id0, 1, id1, 0);

            Assert.IsTrue(changed.Contains(id0));
            Assert.IsTrue(changed.Contains(id1));

            Assert.IsFalse(meta.TryGetLinksFrom(id0, 1, out _));
            Assert.IsFalse(meta.TryGetLinksTo(id1, 0, out _));
        }

        [Test]
        public void InvalidateNode_RemoveAllPorts() {
            var meta = new BlueprintMeta2();

            long id0 = meta.AddNode(typeof(BlueprintSourceTest0));
            long id1 = meta.AddNode(typeof(BlueprintSourceTest2));

            meta.AddPort(id1, Port.Enter());
            meta.AddPort(id1, Port.Exit());

            meta.TryCreateLink(id0, 1, id1, 0);
            meta.InvalidateNode(id1, invalidateLinks: true);

            Assert.AreEqual(0, meta.GetPortCount(id1));
            Assert.IsFalse(meta.TryGetLinksFrom(id0, 1, out _));
            Assert.IsFalse(meta.TryGetLinksTo(id1, 0, out _));
        }

        [Test]
        public void InvalidateNode_RemovePort() {
            var meta = new BlueprintMeta2();

            long id0 = meta.AddNode(typeof(BlueprintSourceTest0));
            long id1 = meta.AddNode(typeof(BlueprintSourceTest0));

            meta.AddPort(id0, Port.Exit());

            meta.TryCreateLink(id0, 1, id1, 0);
            meta.InvalidateNode(id0, invalidateLinks: true);

            Assert.AreEqual(2, meta.GetPortCount(id0));
            Assert.IsTrue(meta.TryGetLinksFrom(id0, 1, out _));
            Assert.IsTrue(meta.TryGetLinksTo(id1, 0, out _));
        }

        [Test]
        public void GetFromLinks() {
            var meta = new BlueprintMeta2();

            long id0 = meta.AddNode(typeof(BlueprintSourceTest0));
            long id1 = meta.AddNode(typeof(BlueprintSourceTest0), Vector2.zero);
            long id2 = meta.AddNode(typeof(BlueprintSourceTest0), Vector2.one);

            meta.TryCreateLink(id0, 1, id1, 0);
            meta.TryCreateLink(id0, 1, id2, 0);

            meta.TryGetLinksFrom(id0, 1, out int i);
            Assert.AreEqual(id1, meta.GetLink(i).nodeId);

            meta.TryGetNextLink(i, out i);
            Assert.AreEqual(id2, meta.GetLink(i).nodeId);

            meta.RemoveLink(id0, 1, id1, 0);
            meta.RemoveLink(id0, 1, id2, 0);

            meta.TryCreateLink(id0, 1, id2, 0);
            meta.TryCreateLink(id0, 1, id1, 0);

            meta.TryGetLinksFrom(id0, 1, out i);
            Assert.AreEqual(id1, meta.GetLink(i).nodeId);

            meta.TryGetNextLink(i, out i);
            Assert.AreEqual(id2, meta.GetLink(i).nodeId);
        }
    }

}
