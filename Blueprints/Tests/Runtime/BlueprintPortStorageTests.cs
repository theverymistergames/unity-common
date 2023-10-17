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

            Assert.IsTrue(storage.TryGetPorts(0L, out int p));

            var port = storage.GetPort(p);
            Assert.IsFalse(port.IsInput());
            Assert.AreEqual(1, storage.GetPortKey(p));

            Assert.IsTrue(storage.TryGetPortIndex(0L, 1, out p));
            Assert.AreEqual(1, storage.GetPortKey(p));

            Assert.IsTrue(storage.TryGetNextPortIndex(p, out p));

            port = storage.GetPort(p);
            Assert.IsTrue(port.IsInput());
            Assert.AreEqual(0, storage.GetPortKey(p));

            Assert.IsTrue(storage.TryGetPortIndex(0L, 0, out p));
            Assert.AreEqual(0, storage.GetPortKey(p));
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
