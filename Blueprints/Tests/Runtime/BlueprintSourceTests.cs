using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Core {

    public class BlueprintSourceTests {

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(100)]
        public void AddNodes(int count) {
            var source = new BlueprintSourceTest0();

            for (int i = 0; i < count; i++) {
                int id = source.AddNode();

                Assert.AreEqual(i + 1, id);
                Assert.AreEqual(i + 1, source.Count);
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(-1)]
        public void SetNodeValue(int value) {
            var source = new BlueprintSourceTest0();
            int id = source.AddNode();

            ref var node = ref source.GetNodeByRef<BlueprintNodeTest0>(id);
            node.intValue = value;

            node = source.GetNodeByRef<BlueprintNodeTest0>(id);
            Assert.AreEqual(value, node.intValue);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(-1)]
        public void AddNodeCopy(int value) {
            var source0 = new BlueprintSourceTest0();
            int id0 = source0.AddNode();

            ref var node0 = ref source0.GetNodeByRef<BlueprintNodeTest0>(id0);
            node0.intValue = value;

            var source1 = new BlueprintSourceTest0();
            int id1 = source1.AddNodeClone(source0, id0);

            ref var node1 = ref source1.GetNodeByRef<BlueprintNodeTest0>(id1);
            Assert.AreEqual(value, node1.intValue);
        }

        [Test]
        public void RemoveNode() {
            var source = new BlueprintSourceTest0();

            int id = source.AddNode();
            source.RemoveNode(id);

            Assert.AreEqual(0, source.Count);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        public void AddRemoveNodes(int size) {
            var source = new BlueprintSourceTest0();
            var addedIds = new List<int>();
            var removedIds = new HashSet<int>();

            const int times = 10;
            const float removePossibility = 0.33f;

            for (int i = 0; i < times; i++) {
                for (int j = 0; j < size / times; j++) {
                    int id = source.AddNode();
                    ref var node = ref source.GetNodeByRef<BlueprintNodeTest0>(id);

                    node.intValue = id + 100;

                    addedIds.Add(id);
                }

                for (int j = 0; j < addedIds.Count; j++) {
                    if (Random.Range(0f, 1f) > removePossibility) continue;

                    int id = addedIds[j];
                    if (removedIds.Contains(id)) continue;

                    source.RemoveNode(id);
                    removedIds.Add(id);
                }

                for (int j = 0; j < addedIds.Count; j++) {
                    int id = addedIds[j];

                    if (removedIds.Contains(id)) {
                        Assert.Throws<KeyNotFoundException>(() => source.GetNodeByRef<BlueprintNodeTest0>(id));
                        continue;
                    }

                    ref var node = ref source.GetNodeByRef<BlueprintNodeTest0>(id);
                    Assert.AreEqual(id + 100, node.intValue);
                }
            }
        }
    }

}
