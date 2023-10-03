using System;
using System.Linq;
using MisterGames.Common.Data;
using NUnit.Framework;

namespace Data {

    public class SerializedTreeMapTests {

        [Test]
        public void AddRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);

            Assert.IsTrue(map.ContainsRoot(0));

            Assert.IsTrue(map.TryGetRoot(0, out int rootGet));
            Assert.AreEqual(root, rootGet);

            Assert.AreEqual(root, map.GetRoot(0));

            Assert.AreEqual(1, map.RootCount);
            Assert.AreEqual(0, map.RootKeys.First());
        }

        [Test]
        public void AddRootDuplicate() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int rootDuplicate = map.GetOrAddRoot(0);

            Assert.AreEqual(root, rootDuplicate);
            Assert.AreEqual(1, map.RootCount);
            Assert.AreEqual(1, map.RootKeys.Count);
        }

        [Test]
        public void RemoveRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            map.RemoveRoot(0);

            Assert.IsFalse(map.ContainsRoot(root));

            Assert.IsFalse(map.TryGetRoot(0, out int rootGet));
            Assert.AreEqual(-1, rootGet);

            Assert.AreEqual(-1, map.GetRoot(0));

            Assert.AreEqual(0, map.RootCount);
            Assert.AreEqual(0, map.RootKeys.Count);
        }

        [Test]
        public void AddChildToRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child = map.GetOrAddChild(root, 0);

            Assert.IsTrue(map.ContainsChild(root, child));

            Assert.IsTrue(map.TryGetParent(child, out int rootGet));
            Assert.AreEqual(root, rootGet);

            Assert.AreEqual(root, map.GetParent(child));

            Assert.IsTrue(map.TryGetChild(root, out int childGet));
            Assert.AreEqual(child, childGet);

            Assert.IsTrue(map.TryGetChild(root, 0, out childGet));
            Assert.AreEqual(child, childGet);

            Assert.AreEqual(child, map.GetChild(root));
            Assert.AreEqual(child, map.GetChild(root, 0));

            Assert.AreEqual(1, map.GetChildCount(root));
        }

        [Test]
        public void AddChildDuplicateToRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(root, 0);
            int child1 = map.GetOrAddChild(root, 0);

            Assert.AreEqual(child0, child1);
            Assert.AreEqual(1, map.GetChildCount(root));
        }

        [Test]
        public void AddTwoChildrenToRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(root, 0);
            int child1 = map.GetOrAddChild(root, 1);

            Assert.IsTrue(map.ContainsChild(root, child1));

            Assert.IsTrue(map.TryGetParent(child1, out int rootGet));
            Assert.AreEqual(root, rootGet);

            Assert.AreEqual(root, map.GetParent(child1));

            Assert.IsTrue(map.TryGetChild(root, 1, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetChild(root, 1));

            Assert.AreEqual(2, map.GetChildCount(root));

            Assert.IsTrue(map.TryGetNext(child0, out child1Get));
            Assert.AreEqual(child1, child1Get);
            Assert.AreEqual(child1, map.GetNext(child0));

            Assert.IsTrue(map.TryGetPrevious(child1, out int child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetPrevious(child1));
        }

        [Test]
        public void AddChildToChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(root, 0);
            int child = map.GetOrAddChild(parent, 0);

            Assert.IsTrue(map.ContainsChild(parent, child));

            Assert.IsTrue(map.TryGetParent(child, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParent(child));

            Assert.IsTrue(map.TryGetChild(parent, 0, out int childGet));
            Assert.AreEqual(child, childGet);

            Assert.AreEqual(child, map.GetChild(parent, 0));

            Assert.AreEqual(1, map.GetChildCount(parent));
        }

        [Test]
        public void AddChildDuplicateToChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(root, 0);
            int child0 = map.GetOrAddChild(parent, 0);
            int child1 = map.GetOrAddChild(parent, 0);

            Assert.AreEqual(child0, child1);
            Assert.AreEqual(1, map.GetChildCount(parent));
        }

        [Test]
        public void AddTwoChildrenToChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(root, 0);
            int child0 = map.GetOrAddChild(parent, 0);
            int child1 = map.GetOrAddChild(parent, 1);

            Assert.IsTrue(map.ContainsChild(parent, child1));

            Assert.IsTrue(map.TryGetParent(child1, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParent(child1));

            Assert.IsTrue(map.TryGetChild(parent, 1, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetChild(parent, 1));

            Assert.AreEqual(2, map.GetChildCount(parent));

            Assert.IsTrue(map.TryGetNext(child0, out child1Get));
            Assert.AreEqual(child1, child1Get);
            Assert.AreEqual(child1, map.GetNext(child0));

            Assert.IsTrue(map.TryGetPrevious(child1, out int child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetPrevious(child1));
        }

        [Test]
        public void RemoveChildFromRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child = map.GetOrAddChild(root, 0);

            map.RemoveChild(ref root, child);

            Assert.IsFalse(map.ContainsChild(root, child));

            Assert.IsFalse(map.TryGetParent(child, out int rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, map.GetParent(child));

            Assert.IsFalse(map.TryGetChild(root, out int childGet));
            Assert.AreEqual(-1, childGet);

            Assert.IsFalse(map.TryGetChild(root, 0, out childGet));
            Assert.AreEqual(-1, childGet);

            Assert.AreEqual(-1, map.GetChild(root));
            Assert.AreEqual(-1, map.GetChild(root, 0));

            Assert.AreEqual(0, map.GetChildCount(root));
        }

        [Test]
        public void RemoveFirstChildOfTwoFromRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(root, 0);
            int child1 = map.GetOrAddChild(root, 1);

            map.RemoveChild(ref root, child0);
            child1 = map.GetChild(root, 1);

            Assert.IsTrue(map.ContainsChild(root, child1));

            Assert.IsTrue(map.TryGetParent(child1, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetParent(child1));

            Assert.IsTrue(map.TryGetChild(root, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.IsTrue(map.TryGetChild(root, 1, out child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetChild(root));

            Assert.AreEqual(1, map.GetChildCount(root));
        }

        [Test]
        public void RemoveSecondChildOfTwoFromRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(root, 0);
            int child1 = map.GetOrAddChild(root, 1);

            map.RemoveChild(ref root, child1);
            child0 = map.GetChild(root, 0);

            Assert.IsTrue(map.ContainsChild(root, child0));

            Assert.IsTrue(map.TryGetParent(child0, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetParent(child0));

            Assert.IsTrue(map.TryGetChild(root, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(root, 0, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.AreEqual(child0, map.GetChild(root));

            Assert.AreEqual(1, map.GetChildCount(root));
        }

        [Test]
        public void RemoveSecondChildOfThreeFromRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(root, 0);
            int child1 = map.GetOrAddChild(root, 1);
            int child2 = map.GetOrAddChild(root, 2);

            map.RemoveChild(ref root, child1);
            child0 = map.GetChild(root, 0);
            child2 = map.GetChild(root, 2);

            Assert.IsTrue(map.ContainsChild(root, child2));

            Assert.IsTrue(map.TryGetParent(child2, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetParent(child2));

            Assert.IsTrue(map.TryGetChild(root, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(root, 0, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(root, 2, out int child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(map.TryGetPrevious(child2, out child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetPrevious(child2));

            Assert.IsTrue(map.TryGetNext(child0, out child2Get));
            Assert.AreEqual(child2, child2Get);
            Assert.AreEqual(child2, map.GetNext(child0));

            Assert.AreEqual(child0, map.GetChild(root));

            Assert.AreEqual(2, map.GetChildCount(root));
        }

        [Test]
        public void RemoveChildFromChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(root, 0);
            int child = map.GetOrAddChild(parent, 0);

            map.RemoveChild(ref parent, child);

            Assert.IsFalse(map.ContainsChild(parent, child));

            Assert.IsFalse(map.TryGetParent(child, out int parentGet));
            Assert.AreEqual(-1, parentGet);

            Assert.IsFalse(map.TryGetChild(parent, out int childGet));
            Assert.AreEqual(-1, childGet);

            Assert.IsFalse(map.TryGetChild(parent, 0, out childGet));
            Assert.AreEqual(-1, childGet);

            Assert.AreEqual(-1, map.GetChild(parent));
            Assert.AreEqual(-1, map.GetChild(parent, 0));

            Assert.AreEqual(0, map.GetChildCount(parent));
        }

        [Test]
        public void RemoveFirstChildOfTwoFromChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(root, 0);
            int child0 = map.GetOrAddChild(parent, 0);
            int child1 = map.GetOrAddChild(parent, 1);

            map.RemoveChild(ref parent, child0);
            child1 = map.GetChild(parent, 1);

            Assert.IsTrue(map.ContainsChild(parent, child1));

            Assert.IsTrue(map.TryGetParent(child1, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParent(child1));

            Assert.IsTrue(map.TryGetChild(parent, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.IsTrue(map.TryGetChild(parent, 1, out child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetChild(parent));

            Assert.AreEqual(1, map.GetChildCount(parent));
        }

        [Test]
        public void RemoveSecondChildOfTwoFromChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(root, 0);
            int child0 = map.GetOrAddChild(parent, 0);
            int child1 = map.GetOrAddChild(parent, 1);

            map.RemoveChild(ref parent, child1);
            child0 = map.GetChild(parent, 0);

            Assert.IsTrue(map.ContainsChild(parent, child0));

            Assert.IsTrue(map.TryGetParent(child0, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParent(child0));

            Assert.IsTrue(map.TryGetChild(parent, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(parent, 0, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.AreEqual(child0, map.GetChild(parent));

            Assert.AreEqual(1, map.GetChildCount(parent));
        }

        [Test]
        public void RemoveSecondChildOfThreeFromChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(root, 0);
            int child0 = map.GetOrAddChild(parent, 0);
            int child1 = map.GetOrAddChild(parent, 1);
            int child2 = map.GetOrAddChild(parent, 2);

            map.RemoveChild(ref parent, child1);
            child0 = map.GetChild(parent, 0);
            child2 = map.GetChild(parent, 2);

            Assert.IsTrue(map.ContainsChild(parent, child2));

            Assert.IsTrue(map.TryGetParent(child2, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParent(child2));

            Assert.IsTrue(map.TryGetChild(parent, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(parent, 0, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(parent, 2, out int child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(map.TryGetPrevious(child2, out child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetPrevious(child2));

            Assert.IsTrue(map.TryGetNext(child0, out child2Get));
            Assert.AreEqual(child2, child2Get);
            Assert.AreEqual(child2, map.GetNext(child0));

            Assert.AreEqual(child0, map.GetChild(parent));

            Assert.AreEqual(2, map.GetChildCount(parent));
        }

        [Test]
        public void GetNodeKey() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            ref var node = ref map.GetNode(root);

            Assert.AreEqual(0, node.key);

            root = map.GetOrAddRoot(1);
            node = ref map.GetNode(root);

            Assert.AreEqual(1, node.key);
        }

        [Test]
        public void SetGetNodeValue() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            ref var node = ref map.GetNode(root);

            Assert.AreEqual(0f, node.value);

            node.value = 3f;
            Assert.AreEqual(3f, node.value);

            node = ref map.GetNode(root);
            Assert.AreEqual(3f, node.value);

            map.RemoveRoot(0);
            Assert.Throws<IndexOutOfRangeException>(() => { map.GetNode(root); });
        }
    }

}
