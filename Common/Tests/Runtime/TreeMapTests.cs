using System.Collections.Generic;
using MisterGames.Common.Data;
using NUnit.Framework;
using Random = UnityEngine.Random;

namespace Data {

    public class TreeMapTests {

        [Test]
        public void MovePreOrder() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);

            int child_13 = map.GetOrAddNode(13, root);
            int child_12 = map.GetOrAddNode(12, root);
            int child_11 = map.GetOrAddNode(11, root);

            int child_6 = map.GetOrAddNode(6, root);
            int child_6_8 = map.GetOrAddNode(8, child_6);
            int child_6_8_9 = map.GetOrAddNode(9, child_6_8);
            int child_6_8_9_10 = map.GetOrAddNode(10, child_6_8_9);

            int child_6_7 = map.GetOrAddNode(7, child_6);

            int child_1 = map.GetOrAddNode(1, root);
            int child_1_2 = map.GetOrAddNode(2, child_1);
            int child_1_2_5 = map.GetOrAddNode(5, child_1_2);
            int child_1_2_4 = map.GetOrAddNode(4, child_1_2);
            int child_1_2_3 = map.GetOrAddNode(3, child_1_2);

            var tree = map.GetTree(0);
            ref var node = ref tree.GetNode();
            Assert.AreEqual(0, node.key);

            for (int i = 1; i < 14; i++) {
                Assert.IsTrue(tree.MovePreOrder());
                node = ref tree.GetNode();
                Assert.AreEqual(i, node.key);
            }

            Assert.IsFalse(tree.MovePreOrder());
        }

        [Test]
        public void MovePreOrderWithRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);

            int child_13 = map.GetOrAddNode(13, root);
            int child_12 = map.GetOrAddNode(12, root);
            int child_11 = map.GetOrAddNode(11, root);

            int child_6 = map.GetOrAddNode(6, root);
            int child_6_8 = map.GetOrAddNode(8, child_6);
            int child_6_8_9 = map.GetOrAddNode(9, child_6_8);
            int child_6_8_9_10 = map.GetOrAddNode(10, child_6_8_9);

            int child_6_7 = map.GetOrAddNode(7, child_6);

            int child_1 = map.GetOrAddNode(1, root);
            int child_1_2 = map.GetOrAddNode(2, child_1);
            int child_1_2_5 = map.GetOrAddNode(5, child_1_2);
            int child_1_2_4 = map.GetOrAddNode(4, child_1_2);
            int child_1_2_3 = map.GetOrAddNode(3, child_1_2);

            var tree = map.GetTree(0);

            tree.MoveIndex(child_1);
            int index = tree.Index;

            ref var node = ref tree.GetNode();
            Assert.AreEqual(1, node.key);

            for (int i = 2; i < 6; i++) {
                Assert.IsTrue(tree.MovePreOrder(index));
                node = ref tree.GetNode();
                Assert.AreEqual(i, node.key);
            }

            Assert.IsFalse(tree.MovePreOrder(index));

            tree.MoveIndex(child_6);
            index = tree.Index;

            node = ref tree.GetNode();
            Assert.AreEqual(6, node.key);

            for (int i = 7; i < 11; i++) {
                Assert.IsTrue(tree.MovePreOrder(index));
                node = ref tree.GetNode();
                Assert.AreEqual(i, node.key);
            }

            Assert.IsFalse(tree.MovePreOrder(index));

            tree.MoveIndex(child_11);
            index = tree.Index;

            node = ref tree.GetNode();
            Assert.AreEqual(11, node.key);
            Assert.IsFalse(tree.MovePreOrder(index));
        }

        [Test]
        public void ContainsRoot() {
            var map = new TreeMap<int, float>();

            Assert.IsFalse(map.ContainsKey(0));
            Assert.IsFalse(map.TryGetIndex(0, out int rootGet));

            int root = map.GetOrAddNode(0);

            Assert.IsTrue(map.ContainsKey(0));
            Assert.IsTrue(map.ContainsIndex(root));
            Assert.IsTrue(map.TryGetIndex(0, out rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetIndex(0));
            Assert.AreEqual(1, map.Count);

            map.RemoveNode(0);

            Assert.IsFalse(map.ContainsKey(0));
            Assert.IsFalse(map.TryGetIndex(0, out rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, map.GetIndex(0));
            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void ContainsNode() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int child = map.GetOrAddNode(0, root);

            Assert.IsTrue(map.ContainsKey(0, root));
            Assert.IsTrue(map.ContainsIndex(child));
            Assert.IsTrue(map.TryGetIndex(0, root, out int childGet));
            Assert.AreEqual(child, childGet);
            Assert.AreEqual(child, map.GetIndex(0, root));
            Assert.IsTrue(map.TryGetParentIndex(child, out int parentGet));
            Assert.AreEqual(root, parentGet);
            Assert.AreEqual(root, map.GetParentIndex(child));
            Assert.AreEqual(2, map.Count);

            map.RemoveNode(0, root);

            Assert.IsFalse(map.ContainsKey(0, root));
            Assert.IsFalse(map.ContainsIndex(child));
            Assert.IsFalse(map.TryGetIndex(0, root, out childGet));
            Assert.AreEqual(-1, childGet);
            Assert.AreEqual(-1, map.GetIndex(0, root));
            Assert.IsFalse(map.TryGetParentIndex(child, out parentGet));
            Assert.AreEqual(-1, parentGet);
            Assert.AreEqual(-1, map.GetParentIndex(child));
            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void AddRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            Assert.IsTrue(map.ContainsKey(0));
        }

        [Test]
        public void AddRootDuplicate() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int rootDuplicate = map.GetOrAddNode(0);

            Assert.AreEqual(root, rootDuplicate);
            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void RemoveRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            map.RemoveNode(0);

            Assert.IsFalse(map.ContainsKey(0));
        }

        [Test]
        public void AddChildToRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int child = map.GetOrAddNode(0, root);

            Assert.IsTrue(map.TryGetParentIndex(child, out int rootGet));
            Assert.AreEqual(root, rootGet);

            Assert.AreEqual(root, map.GetParentIndex(child));

            Assert.IsTrue(map.TryGetChildIndex(root, out int childGet));
            Assert.AreEqual(child, childGet);

            Assert.IsTrue(map.TryGetIndex(0, root, out childGet));
            Assert.AreEqual(child, childGet);

            Assert.AreEqual(child, map.GetChildIndex(root));
            Assert.AreEqual(child, map.GetIndex(0, root));

            Assert.AreEqual(1, map.GetChildCount(root));

            Assert.AreEqual(2, map.Count);
        }

        [Test]
        public void AddChildDuplicateToRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int child0 = map.GetOrAddNode(0, root);
            int child1 = map.GetOrAddNode(0, root);

            Assert.AreEqual(child0, child1);
            Assert.AreEqual(1, map.GetChildCount(root));

            Assert.AreEqual(2, map.Count);
        }

        [Test]
        public void AddTwoChildrenToRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int child0 = map.GetOrAddNode(0, root);
            int child1 = map.GetOrAddNode(1, root);

            Assert.IsTrue(map.ContainsKey(1, root));
            Assert.IsTrue(map.ContainsIndex(child1));

            Assert.IsTrue(map.TryGetParentIndex(child1, out int rootGet));
            Assert.AreEqual(root, rootGet);

            Assert.AreEqual(root, map.GetParentIndex(child1));

            Assert.IsTrue(map.TryGetIndex(1, root, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetIndex(1, root));

            Assert.AreEqual(2, map.GetChildCount(root));

            Assert.IsTrue(map.TryGetNextIndex(child1, out int child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetNextIndex(child1));

            Assert.IsTrue(map.TryGetPreviousIndex(child0, out child1Get));
            Assert.AreEqual(child1, child1Get);
            Assert.AreEqual(child1, map.GetPreviousIndex(child0));

            Assert.AreEqual(3, map.Count);
        }

        [Test]
        public void AddChildToChild() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int parent = map.GetOrAddNode(0, root);
            int child = map.GetOrAddNode(0, parent);

            Assert.IsTrue(map.ContainsKey(0, parent));
            Assert.IsTrue(map.ContainsIndex(child));

            Assert.IsTrue(map.TryGetParentIndex(child, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParentIndex(child));

            Assert.IsTrue(map.TryGetIndex(0, parent, out int childGet));
            Assert.AreEqual(child, childGet);

            Assert.AreEqual(child, map.GetIndex(0, parent));

            Assert.AreEqual(1, map.GetChildCount(parent));

            Assert.AreEqual(3, map.Count);
        }

        [Test]
        public void AddChildDuplicateToChild() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int parent = map.GetOrAddNode(0, root);
            int child0 = map.GetOrAddNode(0, parent);
            int child1 = map.GetOrAddNode(0, parent);

            Assert.AreEqual(child0, child1);
            Assert.AreEqual(1, map.GetChildCount(parent));

            Assert.AreEqual(3, map.Count);
        }

        [Test]
        public void AddTwoChildrenToChild() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int parent = map.GetOrAddNode(0, root);
            int child0 = map.GetOrAddNode(0, parent);
            int child1 = map.GetOrAddNode(1, parent);

            Assert.IsTrue(map.ContainsKey(1, parent));
            Assert.IsTrue(map.ContainsIndex(child1));

            Assert.IsTrue(map.TryGetParentIndex(child1, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParentIndex(child1));

            Assert.IsTrue(map.TryGetIndex(1, parent, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetIndex(1, parent));

            Assert.AreEqual(2, map.GetChildCount(parent));

            Assert.IsTrue(map.TryGetNextIndex(child1, out int child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetNextIndex(child1));

            Assert.IsTrue(map.TryGetPreviousIndex(child0, out child1Get));
            Assert.AreEqual(child1, child1Get);
            Assert.AreEqual(child1, map.GetPreviousIndex(child0));

            Assert.AreEqual(4, map.Count);
        }

        [Test]
        public void RemoveChildFromRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int child = map.GetOrAddNode(0, root);

            map.RemoveNode(0, ref root);

            Assert.IsFalse(map.ContainsKey(0, root));

            Assert.IsFalse(map.TryGetParentIndex(child, out int rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, map.GetParentIndex(child));

            Assert.IsFalse(map.TryGetChildIndex(root, out int childGet));
            Assert.AreEqual(-1, childGet);

            Assert.IsFalse(map.TryGetIndex(0, root, out childGet));
            Assert.AreEqual(-1, childGet);

            Assert.AreEqual(-1, map.GetChildIndex(root));
            Assert.AreEqual(-1, map.GetIndex(0, root));

            Assert.AreEqual(0, map.GetChildCount(root));

            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void RemoveFirstChildOfTwoFromRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int child0 = map.GetOrAddNode(0, root);
            int child1 = map.GetOrAddNode(1, root);

            map.RemoveNode(0, ref root);
            child1 = map.GetIndex(1, root);

            Assert.IsTrue(map.ContainsKey(1, root));

            Assert.IsTrue(map.TryGetParentIndex(child1, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetParentIndex(child1));

            Assert.IsTrue(map.TryGetChildIndex(root, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.IsTrue(map.TryGetIndex(1, root, out child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetChildIndex(root));

            Assert.AreEqual(1, map.GetChildCount(root));

            Assert.AreEqual(2, map.Count);
        }

        [Test]
        public void RemoveSecondChildOfTwoFromRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int child0 = map.GetOrAddNode(0, root);
            int child1 = map.GetOrAddNode(1, root);

            map.RemoveNode(1, ref root);
            child0 = map.GetIndex(0, root);

            Assert.IsTrue(map.ContainsKey(0, root));

            Assert.IsTrue(map.TryGetParentIndex(child0, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetParentIndex(child0));

            Assert.IsTrue(map.TryGetChildIndex(root, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetIndex(0, root, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.AreEqual(child0, map.GetChildIndex(root));

            Assert.AreEqual(1, map.GetChildCount(root));

            Assert.AreEqual(2, map.Count);
        }

        [Test]
        public void RemoveSecondChildOfThreeFromRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int child0 = map.GetOrAddNode(0, root);
            int child1 = map.GetOrAddNode(1, root);
            int child2 = map.GetOrAddNode(2, root);

            map.RemoveNode(1, ref root);
            child0 = map.GetIndex(0, root);
            child2 = map.GetIndex(2, root);

            Assert.IsTrue(map.ContainsKey(2, root));

            Assert.IsTrue(map.TryGetParentIndex(child2, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetParentIndex(child2));

            Assert.IsTrue(map.TryGetChildIndex(root, out int child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(map.TryGetIndex(0, root, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetIndex(2, root, out child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(map.TryGetPreviousIndex(child0, out child2Get));
            Assert.AreEqual(child2, child2Get);
            Assert.AreEqual(child2, map.GetPreviousIndex(child0));

            Assert.IsTrue(map.TryGetNextIndex(child2, out child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetNextIndex(child2));

            Assert.AreEqual(child2, map.GetChildIndex(root));

            Assert.AreEqual(2, map.GetChildCount(root));

            Assert.AreEqual(3, map.Count);
        }

        [Test]
        public void RemoveChildFromChild() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int parent = map.GetOrAddNode(0, root);
            int child = map.GetOrAddNode(0, parent);

            map.RemoveNode(0, ref parent);

            Assert.IsFalse(map.ContainsKey(0, parent));

            Assert.IsFalse(map.TryGetParentIndex(child, out int parentGet));
            Assert.AreEqual(-1, parentGet);

            Assert.IsFalse(map.TryGetChildIndex(parent, out int childGet));
            Assert.AreEqual(-1, childGet);

            Assert.IsFalse(map.TryGetIndex(0, parent, out childGet));
            Assert.AreEqual(-1, childGet);

            Assert.AreEqual(-1, map.GetChildIndex(parent));
            Assert.AreEqual(-1, map.GetIndex(0, parent));

            Assert.AreEqual(0, map.GetChildCount(parent));

            Assert.AreEqual(2, map.Count);
        }

        [Test]
        public void RemoveFirstChildOfTwoFromChild() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int parent = map.GetOrAddNode(0, root);
            int child0 = map.GetOrAddNode(0, parent);
            int child1 = map.GetOrAddNode(1, parent);

            map.RemoveNode(0, ref parent);
            child1 = map.GetIndex(1, parent);

            Assert.IsTrue(map.ContainsKey(1, parent));

            Assert.IsTrue(map.TryGetParentIndex(child1, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParentIndex(child1));

            Assert.IsTrue(map.TryGetChildIndex(parent, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.IsTrue(map.TryGetIndex(1, parent, out child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetChildIndex(parent));

            Assert.AreEqual(1, map.GetChildCount(parent));

            Assert.AreEqual(3, map.Count);
        }

        [Test]
        public void RemoveSecondChildOfTwoFromChild() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int parent = map.GetOrAddNode(0, root);
            int child0 = map.GetOrAddNode(0, parent);
            int child1 = map.GetOrAddNode(1, parent);

            map.RemoveNode(1, ref parent);
            child0 = map.GetIndex(0, parent);

            Assert.IsTrue(map.ContainsKey(0, parent));

            Assert.IsTrue(map.TryGetParentIndex(child0, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParentIndex(child0));

            Assert.IsTrue(map.TryGetChildIndex(parent, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetIndex(0, parent, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.AreEqual(child0, map.GetChildIndex(parent));

            Assert.AreEqual(1, map.GetChildCount(parent));

            Assert.AreEqual(3, map.Count);
        }

        [Test]
        public void RemoveSecondChildOfThreeFromChild() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int parent = map.GetOrAddNode(0, root);
            int child0 = map.GetOrAddNode(0, parent);
            int child1 = map.GetOrAddNode(1, parent);
            int child2 = map.GetOrAddNode(2, parent);

            map.RemoveNode(1, ref parent);
            child0 = map.GetIndex(0, parent);
            child2 = map.GetIndex(2, parent);

            Assert.IsTrue(map.ContainsKey(2, parent));

            Assert.IsTrue(map.TryGetParentIndex(child2, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParentIndex(child2));

            Assert.IsTrue(map.TryGetChildIndex(parent, out int child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(map.TryGetIndex(0, parent, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetIndex(2, parent, out child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(map.TryGetPreviousIndex(child0, out child2Get));
            Assert.AreEqual(child2, child2Get);
            Assert.AreEqual(child2, map.GetPreviousIndex(child0));

            Assert.IsTrue(map.TryGetNextIndex(child2, out child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetNextIndex(child2));

            Assert.AreEqual(child2, map.GetChildIndex(parent));

            Assert.AreEqual(2, map.GetChildCount(parent));

            Assert.AreEqual(4, map.Count);
        }

        [Test]
        public void ClearChildrenOfRoot() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int child0 = map.GetOrAddNode(0, root);
            int child1 = map.GetOrAddNode(1, root);

            map.ClearChildren(ref root);

            Assert.IsFalse(map.ContainsKey(0, root));
            Assert.IsFalse(map.ContainsKey(1, root));

            Assert.IsFalse(map.TryGetParentIndex(child0, out int rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, map.GetParentIndex(child0));

            Assert.IsFalse(map.TryGetParentIndex(child1, out rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, map.GetParentIndex(child1));

            Assert.IsFalse(map.TryGetChildIndex(root, out int child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.IsFalse(map.TryGetIndex(0, root, out child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.AreEqual(-1, map.GetChildIndex(root));
            Assert.AreEqual(-1, map.GetIndex(0, root));

            Assert.AreEqual(0, map.GetChildCount(root));

            Assert.AreEqual(1, map.Count);
        }

        [Test]
        public void ClearChildrenOfChild() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int parent = map.GetOrAddNode(0, root);
            int child0 = map.GetOrAddNode(0, parent);
            int child1 = map.GetOrAddNode(1, parent);

            map.ClearChildren(ref parent);

            Assert.IsFalse(map.ContainsKey(0, parent));
            Assert.IsFalse(map.ContainsKey(1, parent));

            Assert.IsFalse(map.TryGetParentIndex(child0, out int parentGet));
            Assert.AreEqual(-1, parentGet);
            Assert.AreEqual(-1, map.GetParentIndex(child0));

            Assert.IsFalse(map.TryGetParentIndex(child1, out parentGet));
            Assert.AreEqual(-1, parentGet);
            Assert.AreEqual(-1, map.GetParentIndex(child1));

            Assert.IsFalse(map.TryGetChildIndex(parent, out int child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.IsFalse(map.TryGetIndex(0, parent, out child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.IsFalse(map.TryGetIndex(1, parent, out int child1Get));
            Assert.AreEqual(-1, child1Get);

            Assert.AreEqual(-1, map.GetChildIndex(parent));
            Assert.AreEqual(-1, map.GetIndex(0, parent));

            Assert.AreEqual(0, map.GetChildCount(parent));

            Assert.AreEqual(2, map.Count);
        }

        [Test]
        public void ClearAll() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            int parent = map.GetOrAddNode(0, root);
            int child0 = map.GetOrAddNode(0, parent);
            int child1 = map.GetOrAddNode(1, parent);

            map.Clear();

            Assert.IsFalse(map.ContainsKey(0));
            Assert.IsFalse(map.ContainsKey(0, root));
            Assert.IsFalse(map.ContainsKey(0, parent));
            Assert.IsFalse(map.ContainsKey(1, parent));

            Assert.AreEqual(0, map.GetChildCount(root));
            Assert.AreEqual(0, map.GetChildCount(parent));

            Assert.AreEqual(0, map.Count);
        }

        [Test]
        public void GetNodeKey() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            ref var node = ref map.GetNode(root);

            Assert.AreEqual(0, node.key);

            root = map.GetOrAddNode(1);
            node = ref map.GetNode(root);

            Assert.AreEqual(1, node.key);
        }

        [Test]
        public void SetGetNodeValue() {
            var map = new TreeMap<int, float>();

            int root = map.GetOrAddNode(0);
            ref var node = ref map.GetNode(root);

            Assert.AreEqual(0f, node.value);

            node.value = 3f;
            Assert.AreEqual(3f, node.value);

            node = ref map.GetNode(root);
            Assert.AreEqual(3f, node.value);
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(1, 4)]
        [TestCase(1, 5)]
        [TestCase(2, 2)]
        [TestCase(2, 3)]
        [TestCase(2, 4)]
        [TestCase(2, 5)]
        [TestCase(3, 3)]
        [TestCase(3, 4)]
        [TestCase(3, 5)]
        [TestCase(10, 2)]
        [TestCase(10, 3)]
        [TestCase(10, 4)]
        [TestCase(100, 2)]
        [TestCase(100, 3)]
        [TestCase(100, 5)]
        [TestCase(100, 20)]
        [TestCase(1000, 2)]
        [TestCase(1000, 100)]
        public void AddRemoveNodes(int size, int levels) {
            var map = new TreeMap<int, float>();

            const int iterations = 10;
            const float removePossibility = 0.33f;
            const int keys = 10;

            var addedNodes = new List<HashSet<int>>();
            var removedNodes = new List<HashSet<int>>();

            for (int i = 0; i < levels; i++) {
                addedNodes.Add(new HashSet<int>());
                removedNodes.Add(new HashSet<int>());
            }

            for (int i = 0; i < iterations; i++) {
                for (int j = 0; j < size; j++) {
                    int level = Random.Range(0, levels - 1);
                    int lastParent = -1;

                    for (int l = 0; l <= level; l++) {
                        var added = addedNodes[l];
                        var removed = removedNodes[l];

                        int key = Random.Range(0, keys);
                        if (added.Contains(key) || removed.Contains(key)) break;

                        int index = map.GetOrAddNode(key, lastParent);

                        lastParent = index;
                        added.Add(key);
                    }
                }

                for (int j = 0; j < size; j++) {
                    if (Random.Range(0f, 1f) > removePossibility) continue;

                    int r = 0;
                    int targetKeyIndex = Random.Range(0, map.Keys.Count - 1);
                    int key = -1;

                    foreach (int root in map.Keys) {
                        if (targetKeyIndex != r++) continue;
                        key = root;
                    }

                    if (key < 0) break;

                    int level = Random.Range(0, levels - 1);
                    var tree = map.GetTree(key);

                    for (int l = 0; l <= level; l++) {
                        if (l < level) {
                            int childCount = tree.GetChildCount();
                            if (childCount > 0) {
                                int childIndex = Random.Range(0, childCount);

                                for (int c = 0; c < childCount; c++) {
                                    if (c == 0) tree.MoveChild();
                                    else tree.MoveNext();

                                    if (c == childIndex) break;
                                }

                                continue;
                            }
                        }

                        int parent = tree.GetParentIndex();
                        int root = tree.Index;
                        key = tree.GetKey();

                        while (true) {
                            removedNodes[tree.Level].Add(tree.GetKey());
                            if (!tree.MovePreOrder(root)) break;
                        }


                        map.RemoveNode(key, parent);
                        break;
                    }
                }

                var addedRoots = addedNodes[0];
                var removedRoots = removedNodes[0];

                foreach (int root in addedRoots) {
                    bool expectedContains = !removedRoots.Contains(root);
                    bool actualContains = map.TryGetTree(root, out var tree);

                    Assert.AreEqual(expectedContains, actualContains);

                    if (!actualContains) continue;

                    while (tree.MovePreOrder()) {
                        ref var node = ref tree.GetNode();
                        Assert.IsTrue(addedNodes[tree.Level].Contains(node.key) && !removedNodes[tree.Level].Contains(node.key));
                    }
                }
            }
        }
    }

}
