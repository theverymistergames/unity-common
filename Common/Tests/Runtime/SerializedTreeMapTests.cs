using System.Collections.Generic;
using MisterGames.Common.Data;
using NUnit.Framework;
using Random = UnityEngine.Random;

namespace Data {

    public class SerializedTreeMapTests {

        [Test]
        public void ContainsRoot() {
            var map = new SerializedTreeMap<int, float>();

            Assert.IsFalse(map.ContainsRoot(0));
            Assert.IsFalse(map.TryGetRoot(0, out int rootGet));

            int root = map.GetOrAddRoot(0);

            Assert.IsTrue(map.ContainsRoot(0));
            Assert.IsTrue(map.ContainsNode(root));
            Assert.IsTrue(map.TryGetRoot(0, out rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetRoot(0));
            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(1, map.RootCount);

            map.RemoveRoot(0);

            Assert.IsFalse(map.ContainsRoot(0));
            Assert.IsFalse(map.TryGetRoot(0, out rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, map.GetRoot(0));
            Assert.AreEqual(0, map.Count);
            Assert.AreEqual(0, map.RootCount);
        }

        [Test]
        public void ContainsNode() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child = map.GetOrAddChild(0, root);

            Assert.IsTrue(map.ContainsNode(child));
            Assert.IsTrue(map.TryGetChild(0, root, out int childGet));
            Assert.AreEqual(child, childGet);
            Assert.AreEqual(child, map.GetChild(0, root));
            Assert.IsTrue(map.TryGetParent(child, out int parentGet));
            Assert.AreEqual(root, parentGet);
            Assert.AreEqual(root, map.GetParent(child));
            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(1, map.NodeCount);

            map.RemoveChild(0, root);

            Assert.IsFalse(map.ContainsNode(child));
            Assert.IsFalse(map.TryGetChild(0, root, out childGet));
            Assert.AreEqual(-1, childGet);
            Assert.AreEqual(-1, map.GetChild(0, root));
            Assert.IsFalse(map.TryGetParent(child, out parentGet));
            Assert.AreEqual(-1, parentGet);
            Assert.AreEqual(-1, map.GetParent(child));
            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(0, map.NodeCount);
        }

        [Test]
        public void AddRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);

            Assert.IsTrue(map.ContainsRoot(0));
        }

        [Test]
        public void AddRootDuplicate() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int rootDuplicate = map.GetOrAddRoot(0);

            Assert.AreEqual(root, rootDuplicate);
            Assert.AreEqual(1, map.RootCount);
        }

        [Test]
        public void RemoveRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            map.RemoveRoot(0);

            Assert.IsFalse(map.ContainsRoot(0));
        }

        [Test]
        public void AddChildToRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child = map.GetOrAddChild(0, root);

            Assert.IsTrue(map.TryGetParent(child, out int rootGet));
            Assert.AreEqual(root, rootGet);

            Assert.AreEqual(root, map.GetParent(child));

            Assert.IsTrue(map.TryGetChild(root, out int childGet));
            Assert.AreEqual(child, childGet);

            Assert.IsTrue(map.TryGetChild(0, root, out childGet));
            Assert.AreEqual(child, childGet);

            Assert.AreEqual(child, map.GetChild(root));
            Assert.AreEqual(child, map.GetChild(0, root));

            Assert.AreEqual(1, map.GetChildCount(root));

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(1, map.NodeCount);
        }

        [Test]
        public void AddChildDuplicateToRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(0, root);
            int child1 = map.GetOrAddChild(0, root);

            Assert.AreEqual(child0, child1);
            Assert.AreEqual(1, map.GetChildCount(root));

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(1, map.NodeCount);
        }

        [Test]
        public void AddTwoChildrenToRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(0, root);
            int child1 = map.GetOrAddChild(1, root);

            Assert.IsTrue(map.ContainsChild(1, root));
            Assert.IsTrue(map.ContainsNode(child1));

            Assert.IsTrue(map.TryGetParent(child1, out int rootGet));
            Assert.AreEqual(root, rootGet);

            Assert.AreEqual(root, map.GetParent(child1));

            Assert.IsTrue(map.TryGetChild(1, root, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetChild(1, root));

            Assert.AreEqual(2, map.GetChildCount(root));

            Assert.IsTrue(map.TryGetNext(child0, out child1Get));
            Assert.AreEqual(child1, child1Get);
            Assert.AreEqual(child1, map.GetNext(child0));

            Assert.IsTrue(map.TryGetPrevious(child1, out int child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetPrevious(child1));

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(2, map.NodeCount);
        }

        [Test]
        public void AddChildToChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(0, root);
            int child = map.GetOrAddChild(0, parent);

            Assert.IsTrue(map.ContainsChild(0, parent));
            Assert.IsTrue(map.ContainsNode(child));

            Assert.IsTrue(map.TryGetParent(child, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParent(child));

            Assert.IsTrue(map.TryGetChild(0, parent, out int childGet));
            Assert.AreEqual(child, childGet);

            Assert.AreEqual(child, map.GetChild(0, parent));

            Assert.AreEqual(1, map.GetChildCount(parent));

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(2, map.NodeCount);
        }

        [Test]
        public void AddChildDuplicateToChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(0, root);
            int child0 = map.GetOrAddChild(0, parent);
            int child1 = map.GetOrAddChild(0, parent);

            Assert.AreEqual(child0, child1);
            Assert.AreEqual(1, map.GetChildCount(parent));

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(2, map.NodeCount);
        }

        [Test]
        public void AddTwoChildrenToChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(0, root);
            int child0 = map.GetOrAddChild(0, parent);
            int child1 = map.GetOrAddChild(1, parent);

            Assert.IsTrue(map.ContainsChild(1, parent));
            Assert.IsTrue(map.ContainsNode(child1));

            Assert.IsTrue(map.TryGetParent(child1, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParent(child1));

            Assert.IsTrue(map.TryGetChild(1, parent, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetChild(1, parent));

            Assert.AreEqual(2, map.GetChildCount(parent));

            Assert.IsTrue(map.TryGetNext(child0, out child1Get));
            Assert.AreEqual(child1, child1Get);
            Assert.AreEqual(child1, map.GetNext(child0));

            Assert.IsTrue(map.TryGetPrevious(child1, out int child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetPrevious(child1));

            Assert.AreEqual(4, map.Count);
            Assert.AreEqual(3, map.NodeCount);
        }

        [Test]
        public void RemoveChildFromRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child = map.GetOrAddChild(0, root);

            map.RemoveChild(0, ref root);

            Assert.IsFalse(map.ContainsChild(0, root));

            Assert.IsFalse(map.TryGetParent(child, out int rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, map.GetParent(child));

            Assert.IsFalse(map.TryGetChild(root, out int childGet));
            Assert.AreEqual(-1, childGet);

            Assert.IsFalse(map.TryGetChild(0, root, out childGet));
            Assert.AreEqual(-1, childGet);

            Assert.AreEqual(-1, map.GetChild(root));
            Assert.AreEqual(-1, map.GetChild(0, root));

            Assert.AreEqual(0, map.GetChildCount(root));

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(0, map.NodeCount);
        }

        [Test]
        public void RemoveFirstChildOfTwoFromRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(0, root);
            int child1 = map.GetOrAddChild(1, root);

            map.RemoveChild(0, ref root);
            child1 = map.GetChild(1, root);

            Assert.IsTrue(map.ContainsChild(1, root));

            Assert.IsTrue(map.TryGetParent(child1, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetParent(child1));

            Assert.IsTrue(map.TryGetChild(root, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.IsTrue(map.TryGetChild(1, root, out child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetChild(root));

            Assert.AreEqual(1, map.GetChildCount(root));

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(1, map.NodeCount);
        }

        [Test]
        public void RemoveSecondChildOfTwoFromRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(0, root);
            int child1 = map.GetOrAddChild(1, root);

            map.RemoveChild(1, ref root);
            child0 = map.GetChild(0, root);

            Assert.IsTrue(map.ContainsChild(0, root));

            Assert.IsTrue(map.TryGetParent(child0, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetParent(child0));

            Assert.IsTrue(map.TryGetChild(root, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(0, root, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.AreEqual(child0, map.GetChild(root));

            Assert.AreEqual(1, map.GetChildCount(root));

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(1, map.NodeCount);
        }

        [Test]
        public void RemoveSecondChildOfThreeFromRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(0, root);
            int child1 = map.GetOrAddChild(1, root);
            int child2 = map.GetOrAddChild(2, root);

            map.RemoveChild(1, ref root);
            child0 = map.GetChild(0, root);
            child2 = map.GetChild(2, root);

            Assert.IsTrue(map.ContainsChild(2, root));

            Assert.IsTrue(map.TryGetParent(child2, out int rootGet));
            Assert.AreEqual(root, rootGet);
            Assert.AreEqual(root, map.GetParent(child2));

            Assert.IsTrue(map.TryGetChild(root, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(0, root, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(2, root, out int child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(map.TryGetPrevious(child2, out child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetPrevious(child2));

            Assert.IsTrue(map.TryGetNext(child0, out child2Get));
            Assert.AreEqual(child2, child2Get);
            Assert.AreEqual(child2, map.GetNext(child0));

            Assert.AreEqual(child0, map.GetChild(root));

            Assert.AreEqual(2, map.GetChildCount(root));

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(2, map.NodeCount);
        }

        [Test]
        public void RemoveChildFromChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(0, root);
            int child = map.GetOrAddChild(0, parent);

            map.RemoveChild(0, ref parent);

            Assert.IsFalse(map.ContainsChild(0, parent));

            Assert.IsFalse(map.TryGetParent(child, out int parentGet));
            Assert.AreEqual(-1, parentGet);

            Assert.IsFalse(map.TryGetChild(parent, out int childGet));
            Assert.AreEqual(-1, childGet);

            Assert.IsFalse(map.TryGetChild(0, parent, out childGet));
            Assert.AreEqual(-1, childGet);

            Assert.AreEqual(-1, map.GetChild(parent));
            Assert.AreEqual(-1, map.GetChild(0, parent));

            Assert.AreEqual(0, map.GetChildCount(parent));

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(1, map.NodeCount);
        }

        [Test]
        public void RemoveFirstChildOfTwoFromChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(0, root);
            int child0 = map.GetOrAddChild(0, parent);
            int child1 = map.GetOrAddChild(1, parent);

            map.RemoveChild(0, ref parent);
            child1 = map.GetChild(1, parent);

            Assert.IsTrue(map.ContainsChild(1, parent));

            Assert.IsTrue(map.TryGetParent(child1, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParent(child1));

            Assert.IsTrue(map.TryGetChild(parent, out int child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.IsTrue(map.TryGetChild(1, parent, out child1Get));
            Assert.AreEqual(child1, child1Get);

            Assert.AreEqual(child1, map.GetChild(parent));

            Assert.AreEqual(1, map.GetChildCount(parent));

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(2, map.NodeCount);
        }

        [Test]
        public void RemoveSecondChildOfTwoFromChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(0, root);
            int child0 = map.GetOrAddChild(0, parent);
            int child1 = map.GetOrAddChild(1, parent);

            map.RemoveChild(1, ref parent);
            child0 = map.GetChild(0, parent);

            Assert.IsTrue(map.ContainsChild(0, parent));

            Assert.IsTrue(map.TryGetParent(child0, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParent(child0));

            Assert.IsTrue(map.TryGetChild(parent, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(0, parent, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.AreEqual(child0, map.GetChild(parent));

            Assert.AreEqual(1, map.GetChildCount(parent));

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(2, map.NodeCount);
        }

        [Test]
        public void RemoveSecondChildOfThreeFromChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(0, root);
            int child0 = map.GetOrAddChild(0, parent);
            int child1 = map.GetOrAddChild(1, parent);
            int child2 = map.GetOrAddChild(2, parent);

            map.RemoveChild(1, ref parent);
            child0 = map.GetChild(0, parent);
            child2 = map.GetChild(2, parent);

            Assert.IsTrue(map.ContainsChild(2, parent));

            Assert.IsTrue(map.TryGetParent(child2, out int parentGet));
            Assert.AreEqual(parent, parentGet);
            Assert.AreEqual(parent, map.GetParent(child2));

            Assert.IsTrue(map.TryGetChild(parent, out int child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(0, parent, out child0Get));
            Assert.AreEqual(child0, child0Get);

            Assert.IsTrue(map.TryGetChild(2, parent, out int child2Get));
            Assert.AreEqual(child2, child2Get);

            Assert.IsTrue(map.TryGetPrevious(child2, out child0Get));
            Assert.AreEqual(child0, child0Get);
            Assert.AreEqual(child0, map.GetPrevious(child2));

            Assert.IsTrue(map.TryGetNext(child0, out child2Get));
            Assert.AreEqual(child2, child2Get);
            Assert.AreEqual(child2, map.GetNext(child0));

            Assert.AreEqual(child0, map.GetChild(parent));

            Assert.AreEqual(2, map.GetChildCount(parent));

            Assert.AreEqual(4, map.Count);
            Assert.AreEqual(3, map.NodeCount);
        }

        [Test]
        public void ClearChildrenOfRoot() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int child0 = map.GetOrAddChild(0, root);
            int child1 = map.GetOrAddChild(1, root);

            map.ClearChildren(ref root);

            Assert.IsFalse(map.ContainsChild(0, root));
            Assert.IsFalse(map.ContainsChild(1, root));

            Assert.IsFalse(map.TryGetParent(child0, out int rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, map.GetParent(child0));

            Assert.IsFalse(map.TryGetParent(child1, out rootGet));
            Assert.AreEqual(-1, rootGet);
            Assert.AreEqual(-1, map.GetParent(child1));

            Assert.IsFalse(map.TryGetChild(root, out int child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.IsFalse(map.TryGetChild(0, root, out child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.AreEqual(-1, map.GetChild(root));
            Assert.AreEqual(-1, map.GetChild(0, root));

            Assert.AreEqual(0, map.GetChildCount(root));

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual(0, map.NodeCount);
        }

        [Test]
        public void ClearChildrenOfChild() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(0, root);
            int child0 = map.GetOrAddChild(0, parent);
            int child1 = map.GetOrAddChild(1, parent);

            map.ClearChildren(ref parent);

            Assert.IsFalse(map.ContainsChild(0, parent));
            Assert.IsFalse(map.ContainsChild(1, parent));

            Assert.IsFalse(map.TryGetParent(child0, out int parentGet));
            Assert.AreEqual(-1, parentGet);
            Assert.AreEqual(-1, map.GetParent(child0));

            Assert.IsFalse(map.TryGetParent(child1, out parentGet));
            Assert.AreEqual(-1, parentGet);
            Assert.AreEqual(-1, map.GetParent(child1));

            Assert.IsFalse(map.TryGetChild(parent, out int child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.IsFalse(map.TryGetChild(0, parent, out child0Get));
            Assert.AreEqual(-1, child0Get);

            Assert.IsFalse(map.TryGetChild(1, parent, out int child1Get));
            Assert.AreEqual(-1, child1Get);

            Assert.AreEqual(-1, map.GetChild(parent));
            Assert.AreEqual(-1, map.GetChild(0, parent));

            Assert.AreEqual(0, map.GetChildCount(parent));

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(1, map.NodeCount);
        }

        [Test]
        public void ClearAll() {
            var map = new SerializedTreeMap<int, float>();

            int root = map.GetOrAddRoot(0);
            int parent = map.GetOrAddChild(0, root);
            int child0 = map.GetOrAddChild(0, parent);
            int child1 = map.GetOrAddChild(1, parent);

            map.ClearAll();

            Assert.IsFalse(map.ContainsRoot(0));
            Assert.IsFalse(map.ContainsChild(0, root));
            Assert.IsFalse(map.ContainsChild(0, parent));
            Assert.IsFalse(map.ContainsChild(1, parent));

            Assert.AreEqual(0, map.GetChildCount(root));
            Assert.AreEqual(0, map.GetChildCount(parent));

            Assert.AreEqual(0, map.Count);
            Assert.AreEqual(0, map.RootCount);
            Assert.AreEqual(0, map.NodeCount);
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
            var map = new SerializedTreeMap<int, float>();

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

                        lastParent = l == 0 ? map.GetOrAddRoot(key) : map.GetOrAddChild(key, lastParent);
                        added.Add(key);
                    }
                }

                for (int j = 0; j < size; j++) {
                    if (Random.Range(0f, 1f) > removePossibility) continue;

                    int r = 0;
                    int targetKeyIndex = Random.Range(0, map.RootKeys.Count - 1);
                    int key = -1;

                    foreach (int root in map.RootKeys) {
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

                        int parent = tree.GetParent();
                        int root = tree.Index;

                        while (true) {
                            ref var node = ref tree.GetNode();
                            removedNodes[tree.Level].Add(node.key);

                            if (!tree.MovePreOrder(root)) break;
                        }

                        map.RemoveNode(root, parent);
                        break;
                    }
                }

                map.OptimizeLayout();

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
