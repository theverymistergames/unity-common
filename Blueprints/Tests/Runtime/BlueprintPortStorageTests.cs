using MisterGames.Blueprints;
using MisterGames.Blueprints.Core2;
using NUnit.Framework;

namespace Core {

    public class BlueprintPortStorageTests {

        [Test]
        public void AddPort() {
            var storage = new BlueprintPortStorage();

            storage.AddPort(0L, Port.Enter());
            storage.AddPort(0L, Port.Exit());

            Assert.AreEqual(2, storage.GetPortCount(0L));

            Assert.IsTrue(storage.TryGetPort(0L, 0, out var port));
            Assert.IsTrue(port.IsInput());

            Assert.IsTrue(storage.TryGetPort(0L, 1, out port));
            Assert.IsFalse(port.IsInput());
        }

        [Test]
        public void GetPortCount() {
            var storage = new BlueprintPortStorage();

            storage.AddPort(0L, Port.Enter());
            storage.AddPort(0L, Port.Exit());
            storage.AddPort(0L, Port.Exit());

            Assert.AreEqual(3, storage.GetPortCount(0L));
        }

        [Test]
        public void RemoveNode() {
            var storage = new BlueprintPortStorage();

            storage.AddPort(0L, Port.Enter());
            storage.AddPort(0L, Port.Exit());
            storage.AddPort(0L, Port.Exit());

            storage.RemoveNode(0L);

            Assert.AreEqual(0, storage.GetPortCount(0L));
        }

        [Test]
        public void CreatePortSignatureToIndicesTree() {
            var storage = new BlueprintPortStorage();

            storage.AddPort(0L, Port.Enter());
            storage.AddPort(0L, Port.Exit());
            storage.AddPort(0L, Port.Exit());

            var tree = storage.CreatePortSignatureToIndicesTree(0L);

            Assert.IsTrue(tree.ContainsKey(Port.Enter().GetSignature()));
            Assert.IsTrue(tree.ContainsKey(Port.Exit().GetSignature()));
            Assert.AreEqual(2, tree.Roots.Count);

            Assert.IsTrue(tree.TryGetIndex(Port.Enter().GetSignature(), out int enterRoot));
            Assert.IsTrue(tree.TryGetIndex(Port.Exit().GetSignature(), out int exitRoot));

            Assert.IsTrue(tree.ContainsKey(0, enterRoot));
            Assert.IsTrue(tree.ContainsKey(1, exitRoot));
            Assert.IsTrue(tree.ContainsKey(2, exitRoot));
        }
    }

}
