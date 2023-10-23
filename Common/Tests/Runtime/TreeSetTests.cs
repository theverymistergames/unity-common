using System.Collections.Generic;
using MisterGames.Common.Data;
using NUnit.Framework;
using Random = UnityEngine.Random;

namespace Data {

    public class TreeSetTests {

        [Test]
        public void MovePreOrder() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);

            int child_13 = set.GetOrAddNode(13, root);
            int child_12 = set.GetOrAddNode(12, root);
            int child_11 = set.GetOrAddNode(11, root);

            int child_6 = set.GetOrAddNode(6, root);
            int child_6_8 = set.GetOrAddNode(8, child_6);
            int child_6_8_9 = set.GetOrAddNode(9, child_6_8);
            int child_6_8_9_10 = set.GetOrAddNode(10, child_6_8_9);

            int child_6_7 = set.GetOrAddNode(7, child_6);

            int child_1 = set.GetOrAddNode(1, root);
            int child_1_2 = set.GetOrAddNode(2, child_1);
            int child_1_2_5 = set.GetOrAddNode(5, child_1_2);
            int child_1_2_4 = set.GetOrAddNode(4, child_1_2);
            int child_1_2_3 = set.GetOrAddNode(3, child_1_2);

            var tree = set.GetTree(0);
            Assert.AreEqual(0, tree.GetKey());

            for (int i = 1; i < 14; i++) {
                Assert.IsTrue(tree.MovePreOrder());
                Assert.AreEqual(i, tree.GetKey());
            }

            Assert.IsFalse(tree.MovePreOrder());
        }

        [Test]
        public void MovePreOrderWithRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);

            int child_13 = set.GetOrAddNode(13, root);
            int child_12 = set.GetOrAddNode(12, root);
            int child_11 = set.GetOrAddNode(11, root);

            int child_6 = set.GetOrAddNode(6, root);
            int child_6_8 = set.GetOrAddNode(8, child_6);
            int child_6_8_9 = set.GetOrAddNode(9, child_6_8);
            int child_6_8_9_10 = set.GetOrAddNode(10, child_6_8_9);

            int child_6_7 = set.GetOrAddNode(7, child_6);

            int child_1 = set.GetOrAddNode(1, root);
            int child_1_2 = set.GetOrAddNode(2, child_1);
            int child_1_2_5 = set.GetOrAddNode(5, child_1_2);
            int child_1_2_4 = set.GetOrAddNode(4, child_1_2);
            int child_1_2_3 = set.GetOrAddNode(3, child_1_2);

            var tree = set.GetTree(0);

            tree.MoveNode(child_1);
            int index = tree.Index;

            Assert.AreEqual(1, tree.GetKey());

            for (int i = 2; i < 6; i++) {
                Assert.IsTrue(tree.MovePreOrder(index));
                Assert.AreEqual(i, tree.GetKey());
            }

            Assert.IsFalse(tree.MovePreOrder(index));

            tree.MoveNode(child_6);
            index = tree.Index;

            Assert.AreEqual(6, tree.GetKey());

            for (int i = 7; i < 11; i++) {
                Assert.IsTrue(tree.MovePreOrder(index));
                Assert.AreEqual(i, tree.GetKey());
            }

            Assert.IsFalse(tree.MovePreOrder(index));

            tree.MoveNode(child_11);
            index = tree.Index;

            Assert.AreEqual(11, tree.GetKey());
            Assert.IsFalse(tree.MovePreOrder(index));
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        public void SortChildren(int size) {
            var set = new TreeMap<int, int>();

            int root = set.GetOrAddNode(0);

            for (int i = 0; i < size; i++) {
                int index = set.GetOrAddNode(i, root);
                set.SetValueAt(index, Random.Range(0, size));
            }

            set.SortChildren(root);
            set.TryGetChild(root, out int child);

            int lastValue = set.GetValueAt(child);

            while (set.TryGetNext(child, out child)) {
                int value = set.GetValueAt(child);
                Assert.IsTrue(value >= lastValue);
                lastValue = value;
            }
        }

        [Test]
        public void InsertNextChild() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int child1 = set.InsertNextNode(1, root);
            int child2 = set.InsertNextNode(2, root, child1);
            int child3 = set.InsertNextNode(3, root, child2);

            Assert.AreEqual(child1, set.GetChild(root));
            Assert.AreEqual(child2, set.GetNext(child1));
            Assert.AreEqual(child3, set.GetNext(child2));
        }

        [Test]
        public void ContainsRoot() {
            var set = new TreeSet<int>();

            Assert.IsFalse(set.ContainsNode(0));
            Assert.IsFalse(set.TryGetNode(0, out int rootGet));

            int root = set.GetOrAddNode(0);

            Assert.IsTrue(set.ContainsNode(0));
            Assert.IsTrue(set.ContainsNodeAt(root));
            Assert.IsTrue(set.TryGetNode(0, out rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, set.GetNode(0));
            Assert.AreEqual(1, set.Count);

            set.RemoveNode(0);

            Assert.IsFalse(set.ContainsNode(0));
            Assert.IsFalse(set.TryGetNode(0, out rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, set.GetNode(0));
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void ContainsNode() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int child = set.GetOrAddNode(0, root);

            Assert.IsTrue(set.ContainsNode(0, root));
            Assert.IsTrue(set.ContainsNodeAt(child));
            Assert.IsTrue(set.TryGetNode(0, root, out int childGet));
            Assert.AreEqual(child, childGet);
            Assert.AreEqual(child, set.GetNode(0, root));
            Assert.IsTrue(set.TryGetParent(child, out int parentGet));
            Assert.AreEqual(root, parentGet);
            Assert.AreEqual(root, set.GetParent(child));
            Assert.AreEqual(2, set.Count);

            set.RemoveNode(0, root);

            Assert.IsFalse(set.ContainsNode(0, root));
            Assert.IsFalse(set.ContainsNodeAt(child));
            Assert.IsFalse(set.TryGetNode(0, root, out childGet));
            Assert.AreEqual(-1, childGet);
            Assert.AreEqual(-1, set.GetNode(0, root));
            Assert.IsFalse(set.TryGetParent(child, out parentGet));
            Assert.AreEqual(-1, parentGet);
            Assert.AreEqual(-1, set.GetParent(child));
            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void AddRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            Assert.IsTrue(set.ContainsNode(0));
        }

        [Test]
        public void AddRootDuplicate() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int rootDuplicate = set.GetOrAddNode(0);

            Assert.AreEqual(root, rootDuplicate);
            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void RemoveRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            set.RemoveNode(0);

            Assert.IsFalse(set.ContainsNode(0));
        }

        [Test]
        public void AddChildToRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int child = set.GetOrAddNode(0, root);

            Assert.IsTrue(set.TryGetParent(child, out int rootGet));
            Assert.AreEqual(root, rootGet);

            Assert.AreEqual(root, set.GetParent(child));

            Assert.IsTrue(set.TryGetChild(root, out int childGet));
            Assert.AreEqual(child, childGet);

            Assert.IsTrue(set.TryGetNode(0, root, out childGet));
            Assert.AreEqual(child, childGet);

            Assert.AreEqual(child, set.GetChild(root));
            Assert.AreEqual(child, set.GetNode(0, root));

            Assert.AreEqual(1, set.GetChildrenCount(root));

            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public void AddChildDuplicateToRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int child0 = set.GetOrAddNode(0, root);
            int child1 = set.GetOrAddNode(0, root);

            Assert.AreEqual(child0, child1);
            Assert.AreEqual(1, set.GetChildrenCount(root));

            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public void AddTwoChildrenToRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int child0 = set.GetOrAddNode(0, root);
            int child1 = set.GetOrAddNode(1, root);

            Assert.IsTrue(set.ContainsNode(1, root));
            Assert.IsTrue(set.ContainsNodeAt(child1));

            Assert.IsTrue(set.TryGetParent(child1, out int rootGet));
            Assert.AreEqual(root, rootGet);

            Assert.AreEqual(root, set.GetParent(child1));

            Assert.IsTrue(set.TryGetNode(1, root, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, set.GetNode(1, root));

            Assert.AreEqual(2, set.GetChildrenCount(root));

            Assert.IsTrue(set.TryGetNext(child1, out int child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, set.GetNext(child1));

            Assert.IsTrue(set.TryGetPrevious(child0, out child1Get));
            Assert.AreEqual(child1, child1Get);
            Assert.AreEqual(child1, set.GetPrevious(child0));

            Assert.AreEqual(3, set.Count);
        }

        [Test]
        public void AddChildToChild() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int parent = set.GetOrAddNode(0, root);
            int child = set.GetOrAddNode(0, parent);

            Assert.IsTrue(set.ContainsNode(0, parent));
            Assert.IsTrue(set.ContainsNodeAt(child));

            Assert.IsTrue(set.TryGetParent(child, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, set.GetParent(child));

            Assert.IsTrue(set.TryGetNode(0, parent, out int childGet));
            Assert.AreEqual(child, childGet);

            Assert.AreEqual(child, set.GetNode(0, parent));

            Assert.AreEqual(1, set.GetChildrenCount(parent));

            Assert.AreEqual(3, set.Count);
        }

        [Test]
        public void AddChildDuplicateToChild() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int parent = set.GetOrAddNode(0, root);
            int child0 = set.GetOrAddNode(0, parent);
            int child1 = set.GetOrAddNode(0, parent);

            Assert.AreEqual(child0, child1);
            Assert.AreEqual(1, set.GetChildrenCount(parent));

            Assert.AreEqual(3, set.Count);
        }

        [Test]
        public void AddTwoChildrenToChild() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int parent = set.GetOrAddNode(0, root);
            int child0 = set.GetOrAddNode(0, parent);
            int child1 = set.GetOrAddNode(1, parent);

            Assert.IsTrue(set.ContainsNode(1, parent));
            Assert.IsTrue(set.ContainsNodeAt(child1));

            Assert.IsTrue(set.TryGetParent(child1, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, set.GetParent(child1));

            Assert.IsTrue(set.TryGetNode(1, parent, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, set.GetNode(1, parent));

            Assert.AreEqual(2, set.GetChildrenCount(parent));

            Assert.IsTrue(set.TryGetNext(child1, out int child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, set.GetNext(child1));

            Assert.IsTrue(set.TryGetPrevious(child0, out child1Get));
            Assert.AreEqual(child1, child1Get);
            Assert.AreEqual(child1, set.GetPrevious(child0));

            Assert.AreEqual(4, set.Count);
        }

        [Test]
        public void RemoveChildFromRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int child = set.GetOrAddNode(0, root);

            set.RemoveNode(0, ref root);

            Assert.IsFalse(set.ContainsNode(0, root));

            Assert.IsFalse(set.TryGetParent(child, out int rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, set.GetParent(child));

            Assert.IsFalse(set.TryGetChild(root, out int childGet));
            Assert.AreEqual(-1, childGet);

            Assert.IsFalse(set.TryGetNode(0, root, out childGet));
            Assert.AreEqual(-1, childGet);

            Assert.AreEqual(-1, set.GetChild(root));
            Assert.AreEqual(-1, set.GetNode(0, root));

            Assert.AreEqual(0, set.GetChildrenCount(root));

            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void RemoveFirstChildOfTwoFromRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int child0 = set.GetOrAddNode(0, root);
            int child1 = set.GetOrAddNode(1, root);

            set.RemoveNode(0, ref root);
            child1 = set.GetNode(1, root);

            Assert.IsTrue(set.ContainsNode(1, root));

            Assert.IsTrue(set.TryGetParent(child1, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, set.GetParent(child1));

            Assert.IsTrue(set.TryGetChild(root, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.IsTrue(set.TryGetNode(1, root, out child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, set.GetChild(root));

            Assert.AreEqual(1, set.GetChildrenCount(root));

            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public void RemoveSecondChildOfTwoFromRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int child0 = set.GetOrAddNode(0, root);
            int child1 = set.GetOrAddNode(1, root);

            set.RemoveNode(1, ref root);
            child0 = set.GetNode(0, root);

            Assert.IsTrue(set.ContainsNode(0, root));

            Assert.IsTrue(set.TryGetParent(child0, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, set.GetParent(child0));

            Assert.IsTrue(set.TryGetChild(root, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(set.TryGetNode(0, root, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.AreEqual(child0, set.GetChild(root));

            Assert.AreEqual(1, set.GetChildrenCount(root));

            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public void RemoveSecondChildOfThreeFromRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int child0 = set.GetOrAddNode(0, root);
            int child1 = set.GetOrAddNode(1, root);
            int child2 = set.GetOrAddNode(2, root);

            set.RemoveNode(1, ref root);
            child0 = set.GetNode(0, root);
            child2 = set.GetNode(2, root);

            Assert.IsTrue(set.ContainsNode(2, root));

            Assert.IsTrue(set.TryGetParent(child2, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, set.GetParent(child2));

            Assert.IsTrue(set.TryGetChild(root, out int child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(set.TryGetNode(0, root, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(set.TryGetNode(2, root, out child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(set.TryGetPrevious(child0, out child2Get));
            Assert.AreEqual(child2, child2Get);
            Assert.AreEqual(child2, set.GetPrevious(child0));

            Assert.IsTrue(set.TryGetNext(child2, out child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, set.GetNext(child2));

            Assert.AreEqual(child2, set.GetChild(root));

            Assert.AreEqual(2, set.GetChildrenCount(root));

            Assert.AreEqual(3, set.Count);
        }

        [Test]
        public void RemoveChildFromChild() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int parent = set.GetOrAddNode(0, root);
            int child = set.GetOrAddNode(0, parent);

            set.RemoveNode(0, ref parent);

            Assert.IsFalse(set.ContainsNode(0, parent));

            Assert.IsFalse(set.TryGetParent(child, out int parentGet));
            Assert.AreEqual(-1, parentGet);

            Assert.IsFalse(set.TryGetChild(parent, out int childGet));
            Assert.AreEqual(-1, childGet);

            Assert.IsFalse(set.TryGetNode(0, parent, out childGet));
            Assert.AreEqual(-1, childGet);

            Assert.AreEqual(-1, set.GetChild(parent));
            Assert.AreEqual(-1, set.GetNode(0, parent));

            Assert.AreEqual(0, set.GetChildrenCount(parent));

            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public void RemoveFirstChildOfTwoFromChild() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int parent = set.GetOrAddNode(0, root);
            int child0 = set.GetOrAddNode(0, parent);
            int child1 = set.GetOrAddNode(1, parent);

            set.RemoveNode(0, ref parent);
            child1 = set.GetNode(1, parent);

            Assert.IsTrue(set.ContainsNode(1, parent));

            Assert.IsTrue(set.TryGetParent(child1, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, set.GetParent(child1));

            Assert.IsTrue(set.TryGetChild(parent, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.IsTrue(set.TryGetNode(1, parent, out child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, set.GetChild(parent));

            Assert.AreEqual(1, set.GetChildrenCount(parent));

            Assert.AreEqual(3, set.Count);
        }

        [Test]
        public void RemoveSecondChildOfTwoFromChild() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int parent = set.GetOrAddNode(0, root);
            int child0 = set.GetOrAddNode(0, parent);
            int child1 = set.GetOrAddNode(1, parent);

            set.RemoveNode(1, ref parent);
            child0 = set.GetNode(0, parent);

            Assert.IsTrue(set.ContainsNode(0, parent));

            Assert.IsTrue(set.TryGetParent(child0, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, set.GetParent(child0));

            Assert.IsTrue(set.TryGetChild(parent, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(set.TryGetNode(0, parent, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.AreEqual(child0, set.GetChild(parent));

            Assert.AreEqual(1, set.GetChildrenCount(parent));

            Assert.AreEqual(3, set.Count);
        }

        [Test]
        public void RemoveSecondChildOfThreeFromChild() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int parent = set.GetOrAddNode(0, root);
            int child0 = set.GetOrAddNode(0, parent);
            int child1 = set.GetOrAddNode(1, parent);
            int child2 = set.GetOrAddNode(2, parent);

            set.RemoveNode(1, ref parent);
            child0 = set.GetNode(0, parent);
            child2 = set.GetNode(2, parent);

            Assert.IsTrue(set.ContainsNode(2, parent));

            Assert.IsTrue(set.TryGetParent(child2, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, set.GetParent(child2));

            Assert.IsTrue(set.TryGetChild(parent, out int child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(set.TryGetNode(0, parent, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(set.TryGetNode(2, parent, out child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(set.TryGetPrevious(child0, out child2Get));
            Assert.AreEqual(child2, child2Get);
            Assert.AreEqual(child2, set.GetPrevious(child0));

            Assert.IsTrue(set.TryGetNext(child2, out child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, set.GetNext(child2));

            Assert.AreEqual(child2, set.GetChild(parent));

            Assert.AreEqual(2, set.GetChildrenCount(parent));

            Assert.AreEqual(4, set.Count);
        }

        [Test]
        public void ClearChildrenOfRoot() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int child0 = set.GetOrAddNode(0, root);
            int child1 = set.GetOrAddNode(1, root);

            set.ClearChildren(ref root);

            Assert.IsFalse(set.ContainsNode(0, root));
            Assert.IsFalse(set.ContainsNode(1, root));

            Assert.IsFalse(set.TryGetParent(child0, out int rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, set.GetParent(child0));

            Assert.IsFalse(set.TryGetParent(child1, out rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, set.GetParent(child1));

            Assert.IsFalse(set.TryGetChild(root, out int child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.IsFalse(set.TryGetNode(0, root, out child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.AreEqual(-1, set.GetChild(root));
            Assert.AreEqual(-1, set.GetNode(0, root));

            Assert.AreEqual(0, set.GetChildrenCount(root));

            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void ClearChildrenOfChild() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int parent = set.GetOrAddNode(0, root);
            int child0 = set.GetOrAddNode(0, parent);
            int child1 = set.GetOrAddNode(1, parent);

            set.ClearChildren(ref parent);

            Assert.IsFalse(set.ContainsNode(0, parent));
            Assert.IsFalse(set.ContainsNode(1, parent));

            Assert.IsFalse(set.TryGetParent(child0, out int parentGet));
            Assert.AreEqual(-1, parentGet);
            Assert.AreEqual(-1, set.GetParent(child0));

            Assert.IsFalse(set.TryGetParent(child1, out parentGet));
            Assert.AreEqual(-1, parentGet);
            Assert.AreEqual(-1, set.GetParent(child1));

            Assert.IsFalse(set.TryGetChild(parent, out int child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.IsFalse(set.TryGetNode(0, parent, out child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.IsFalse(set.TryGetNode(1, parent, out int child1Get));
            Assert.AreEqual(-1, child1Get);

            Assert.AreEqual(-1, set.GetChild(parent));
            Assert.AreEqual(-1, set.GetNode(0, parent));

            Assert.AreEqual(0, set.GetChildrenCount(parent));

            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public void ClearAll() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            int parent = set.GetOrAddNode(0, root);
            int child0 = set.GetOrAddNode(0, parent);
            int child1 = set.GetOrAddNode(1, parent);

            set.Clear();

            Assert.IsFalse(set.ContainsNode(0));
            Assert.IsFalse(set.ContainsNode(0, root));
            Assert.IsFalse(set.ContainsNode(0, parent));
            Assert.IsFalse(set.ContainsNode(1, parent));

            Assert.AreEqual(0, set.GetChildrenCount(root));
            Assert.AreEqual(0, set.GetChildrenCount(parent));

            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void GetNodeKey() {
            var set = new TreeSet<int>();

            int root = set.GetOrAddNode(0);
            Assert.AreEqual(0, set.GetKeyAt(root));

            root = set.GetOrAddNode(1);
            Assert.AreEqual(1, set.GetKeyAt(root));
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
            var set = new TreeSet<int>();

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

                        int index = set.GetOrAddNode(key, lastParent);

                        lastParent = index;
                        added.Add(key);
                    }
                }

                for (int j = 0; j < size; j++) {
                    if (Random.Range(0f, 1f) > removePossibility) continue;

                    int r = 0;
                    int targetKeyIndex = Random.Range(0, set.Roots.Count - 1);
                    int key = -1;

                    foreach (int root in set.Roots) {
                        if (targetKeyIndex != r++) continue;
                        key = root;
                    }

                    if (key < 0) break;

                    int level = Random.Range(0, levels - 1);
                    var tree = set.GetTree(key);

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

                        int parent = tree.GetParent();
                        int root = tree.Index;
                        key = tree.GetKey();

                        while (true) {
                            removedNodes[tree.Level].Add(tree.GetKey());
                            if (!tree.MovePreOrder(root)) break;
                        }


                        set.RemoveNode(key, parent);
                        break;
                    }
                }

                var addedRoots = addedNodes[0];
                var removedRoots = removedNodes[0];

                foreach (int root in addedRoots) {
                    bool expectedContains = !removedRoots.Contains(root);
                    bool actualContains = set.TryGetTree(root, out var tree);

                    Assert.AreEqual(expectedContains, actualContains);

                    if (!actualContains) continue;

                    while (tree.MovePreOrder()) {
                        Assert.IsTrue(addedNodes[tree.Level].Contains(tree.GetKey()) &&
                                      !removedNodes[tree.Level].Contains(tree.GetKey()));
                    }
                }
            }
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
        public void CopyTree(int size, int levels) {
            var set = new TreeSet<int>();

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

                        int index = set.GetOrAddNode(key, lastParent);

                        lastParent = index;
                        added.Add(key);
                    }
                }

                for (int j = 0; j < size; j++) {
                    if (Random.Range(0f, 1f) > removePossibility) continue;

                    int r = 0;
                    int targetKeyIndex = Random.Range(0, set.Roots.Count - 1);
                    int key = -1;

                    foreach (int root in set.Roots) {
                        if (targetKeyIndex != r++) continue;
                        key = root;
                    }

                    if (key < 0) break;

                    int level = Random.Range(0, levels - 1);
                    var tree = set.GetTree(key);

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

                        int parent = tree.GetParent();
                        int root = tree.Index;
                        key = tree.GetKey();

                        while (true) {
                            removedNodes[tree.Level].Add(tree.GetKey());
                            if (!tree.MovePreOrder(root)) break;
                        }

                        set.RemoveNode(key, parent);
                        break;
                    }
                }

                const int attemptsMax = 100;
                int attemptsCount = 0;
                int copyRoot = -1;

                while (!set.ContainsNodeAt(copyRoot) && attemptsCount++ < attemptsMax) {
                    copyRoot = Random.Range(0, set.Count);
                }

                if (!set.ContainsNodeAt(copyRoot)) continue;

                int levelOffset = set.GetNodeDepth(copyRoot);
                var copy = set.Copy(copyRoot);

                var addedRoots = addedNodes[levelOffset];
                var removedRoots = removedNodes[levelOffset];

                foreach (int root in copy.Roots) {
                    Assert.IsTrue(addedRoots.Contains(root));
                    Assert.IsFalse(removedRoots.Contains(root));

                    var tree = copy.GetTree(root);

                    while (tree.MovePreOrder()) {
                        Assert.IsTrue(addedNodes[tree.Level + levelOffset].Contains(tree.GetKey()) &&
                                      !removedNodes[tree.Level + levelOffset].Contains(tree.GetKey()));
                    }
                }

                copy = set.Copy(copyRoot, includeRoot: false);
                if (copy.Count <= 0) continue;

                levelOffset++;
                addedRoots = addedNodes[levelOffset];
                removedRoots = removedNodes[levelOffset];

                foreach (int root in copy.Roots) {
                    Assert.IsTrue(addedRoots.Contains(root));
                    Assert.IsFalse(removedRoots.Contains(root));

                    var tree = copy.GetTree(root);

                    while (tree.MovePreOrder()) {
                        Assert.IsTrue(addedNodes[tree.Level + levelOffset].Contains(tree.GetKey()) &&
                                      !removedNodes[tree.Level + levelOffset].Contains(tree.GetKey()));
                    }
                }
            }
        }
    }

}
