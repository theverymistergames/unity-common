using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Core {

    public class BlueprintNodeFactoryTests {

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(100)]
        public void AddElements(int count) {
            var factory = new BlueprintNodeTestFactory();

            for (int i = 0; i < count; i++) {
                int id = factory.AddBlueprintNodeData();

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
            var factory = new BlueprintNodeTestFactory();
            int id = factory.AddBlueprintNodeData();

            ref var dataByRef = ref factory.GetData<BlueprintNodeTestData>(id);
            dataByRef.intValue = value;

            var data = factory.GetData<BlueprintNodeTestData>(id);
            Assert.AreEqual(value, data.intValue);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(-1)]
        public void AddElementCopy(int value) {
            var factory0 = new BlueprintNodeTestFactory();
            int id0 = factory0.AddBlueprintNodeData();

            ref var data0 = ref factory0.GetData<BlueprintNodeTestData>(id0);
            data0.intValue = value;

            var factory1 = new BlueprintNodeTestFactory();
            int id1 = factory1.AddBlueprintNodeDataCopy(factory0, id0);

            ref var data1 = ref factory1.GetData<BlueprintNodeTestData>(id1);
            Assert.AreEqual(value, data1.intValue);
        }

        [Test]
        public void RemoveElement() {
            var factory = new BlueprintNodeTestFactory();

            int id = factory.AddBlueprintNodeData();
            factory.RemoveBlueprintNodeData(id);

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
        public void OptimizeDataLayout(int size) {
            var factory = new BlueprintNodeTestFactory();

            for (int i = 0; i < size; i++) {
                int id = factory.AddBlueprintNodeData();
                ref var data = ref factory.GetData<BlueprintNodeTestData>(id);

                data.intValue = id + 100;
            }

            var removedIds = new HashSet<int>();

            for (int i = 1; i <= size; i++) {
                if (Random.Range(0f, 1f) < 0.5f) continue;

                factory.RemoveBlueprintNodeData(i);
                removedIds.Add(i);
            }

            for (int i = 1; i <= size; i++) {
                ref var data = ref factory.GetData<BlueprintNodeTestData>(i);
                int expected = removedIds.Contains(i) ? 0 : i + 100;

                Assert.AreEqual(expected, data.intValue);
            }
        }
    }

}
