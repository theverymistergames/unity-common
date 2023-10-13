using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Core {

    public class BlueprintFactoryTests {

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(100)]
        public void AddElements(int count) {
            var factory = new BlueprintSourceTest0();

            for (int i = 0; i < count; i++) {
                int id = factory.AddNode();

                Assert.AreEqual(i + 1, id);
                Assert.AreEqual(i + 1, factory.Count);
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(-1)]
        public void SetElementValue(int value) {
            var factory = new BlueprintSourceTest0();
            int id = factory.AddNode();

            ref var node = ref factory.GetNode<BlueprintNodeTest0>(id);
            node.intValue = value;

            node = factory.GetNode<BlueprintNodeTest0>(id);
            Assert.AreEqual(value, node.intValue);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(-1)]
        public void AddElementCopy(int value) {
            var factory0 = new BlueprintSourceTest0();
            int id0 = factory0.AddNode();

            ref var node0 = ref factory0.GetNode<BlueprintNodeTest0>(id0);
            node0.intValue = value;

            var factory1 = new BlueprintSourceTest0();
            int id1 = factory1.AddNodeCopy(factory0, id0);

            ref var node1 = ref factory1.GetNode<BlueprintNodeTest0>(id1);
            Assert.AreEqual(value, node1.intValue);
        }

        [Test]
        public void RemoveElement() {
            var factory = new BlueprintSourceTest0();

            int id = factory.AddNode();
            factory.RemoveNode(id);

            Assert.AreEqual(0, factory.Count);
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
        public void AddRemoveElements(int size) {
            var factory = new BlueprintSourceTest0();
            var addedIds = new List<int>();
            var removedIds = new HashSet<int>();

            const int times = 10;
            const float removePossibility = 0.33f;

            for (int i = 0; i < times; i++) {
                for (int j = 0; j < size / times; j++) {
                    int id = factory.AddNode();
                    ref var node = ref factory.GetNode<BlueprintNodeTest0>(id);

                    node.intValue = id + 100;

                    addedIds.Add(id);
                }

                for (int j = 0; j < addedIds.Count; j++) {
                    if (Random.Range(0f, 1f) > removePossibility) continue;

                    int id = addedIds[j];
                    if (removedIds.Contains(id)) continue;

                    factory.RemoveNode(id);
                    removedIds.Add(id);
                }

                for (int j = 0; j < addedIds.Count; j++) {
                    int id = addedIds[j];

                    if (removedIds.Contains(id)) {
                        Assert.Throws<KeyNotFoundException>(() => factory.GetNode<BlueprintNodeTest0>(id));
                        continue;
                    }

                    ref var node = ref factory.GetNode<BlueprintNodeTest0>(id);
                    Assert.AreEqual(id + 100, node.intValue);
                }
            }
        }
    }

}
