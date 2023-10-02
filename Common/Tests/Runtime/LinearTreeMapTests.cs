using System;
using MisterGames.Common.Data;
using NUnit.Framework;

namespace Data {

    public class LinearTreeMapTests {

        [Test]
        public void AddRoot() {
            var map = new LinearTreeMap<int, float>();

            var root = map.GetOrAddRoot(0);

            Assert.IsTrue(map.ContainsRoot(0));
            Assert.IsTrue(map.ContainsNode(root.index));
            Assert.IsTrue(map.TryGetRoot(0, out var rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetRoot(0));
        }

        [Test]
        public void AddRootDuplicate() {
            var map = new LinearTreeMap<int, float>();

            var root = map.GetOrAddRoot(0);
            var rootDuplicate = map.GetOrAddRoot(0);

            Assert.AreEqual(root, rootDuplicate);
        }

        [Test]
        public void RemoveRoot() {
            var map = new LinearTreeMap<int, float>();

            var root = map.GetOrAddRoot(0);
            map.SetValue(root.index, 3f);

            map.RemoveRoot(0);

            Assert.IsFalse(map.ContainsNode(root.index));
            Assert.IsFalse(map.TryGetValue(root.index, out float value));
            Assert.AreEqual(0f, value);
            Assert.AreEqual(0f, map.GetValue(root.index));
            Assert.Throws<IndexOutOfRangeException>(() => { map.GetValueByRef(root.index); });
            Assert.IsFalse(map.TrySetValue(root.index, 1f));
        }

        [Test]
        public void AddChildNode() {
            var map = new LinearTreeMap<int, float>();

            var root = map.GetOrAddRoot(0);

            Assert.IsTrue(map.TryAppendChild(root.index, out var child));

            Assert.IsTrue(map.ContainsNode(child.index));
            Assert.IsTrue(map.TryGetNode(child.index, out var childGet));
            Assert.AreEqual(child, childGet);
            Assert.AreEqual(child, map.GetNode(child.index));

            root = map.GetOrAddRoot(0);
            Assert.AreEqual(map.GetNode(root.child), map.GetNode(child.index));
        }

        [Test]
        public void AddChildren() {
            var map = new LinearTreeMap<int, float>();

            var root = map.GetOrAddRoot(0);
            var child0 = map.AppendChild(root.index);
            var child1 = map.AppendChild(root.index);

            //parent = map.GetNode(parent.index);
            //Assert.AreEqual(map.GetNode(root.child), map.GetNode(child.index));
        }

        [Test]
        public void AddNextNode() {
            var map = new LinearTreeMap<int, float>();

            var root = map.GetOrAddRoot(0);

            Assert.IsTrue(map.TryAppendNext(root.index, out var next));

            Assert.IsTrue(map.ContainsNode(next.index));
            Assert.IsTrue(map.TryGetNode(next.index, out var nextGet));
            Assert.AreEqual(next, nextGet);
            Assert.AreEqual(next, map.GetNode(next.index));

            root = map.GetOrAddRoot(0);
            Assert.AreEqual(map.GetNode(root.next), map.GetNode(next.index));
        }

        [Test]
        public void RemoveNode() {
            var map = new LinearTreeMap<int, float>();

            var root = map.GetOrAddRoot(0);
            var node = map.AppendChild(root.index);

            map.SetValue(node.index, 3f);

            map.RemoveNode(node.index);

            Assert.IsFalse(map.TryGetValue(node.index, out float value));
            Assert.AreEqual(0f, value);
            Assert.AreEqual(0f, map.GetValue(node.index));
            Assert.Throws<IndexOutOfRangeException>(() => { map.GetValueByRef(node.index); });
            Assert.IsFalse(map.TrySetValue(node.index, 1f));
        }

        [Test]
        public void SetGetValue() {
            var map = new LinearTreeMap<int, float>();

            var root = map.GetOrAddRoot(0);

            Assert.AreEqual(0f, map.GetValue(root.index));

            map.SetValue(root.index, 3f);
            Assert.AreEqual(3f, map.GetValue(root.index));
        }

        [Test]
        public void SetGetValueByRef() {
            var map = new LinearTreeMap<int, float>();

            var root = map.GetOrAddRoot(0);
            map.SetValue(root.index, 3f);

            ref float result = ref map.GetValueByRef(root.index);
            Assert.AreEqual(3f, result);

            map.SetValue(root.index, 4f);
            Assert.AreEqual(4f, result);
        }


    }

}
