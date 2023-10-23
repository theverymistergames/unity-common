using MisterGames.Blueprints;
using MisterGames.Blueprints.Core2;
using NUnit.Framework;

namespace Core {

    public class BlueprintPortStorageTests {

        [Test]
        public void AddPort() {
            var storage = new BlueprintPortStorage();
            var id = new NodeId(0, 0);
            
            storage.AddPort(id, Port.Enter());
            storage.AddPort(id, Port.Exit());

            Assert.AreEqual(2, storage.GetPortCount(id));

            Assert.IsTrue(storage.TryGetPort(id, 0, out var port));
            Assert.IsTrue(port.IsInput());

            Assert.IsTrue(storage.TryGetPort(id, 1, out port));
            Assert.IsFalse(port.IsInput());
        }

        [Test]
        public void GetPortCount() {
            var storage = new BlueprintPortStorage();
            var id = new NodeId(0, 0);
            
            storage.AddPort(id, Port.Enter());
            storage.AddPort(id, Port.Exit());
            storage.AddPort(id, Port.Exit());

            Assert.AreEqual(3, storage.GetPortCount(id));
        }

        [Test]
        public void RemoveNode() {
            var storage = new BlueprintPortStorage();
            var id = new NodeId(0, 0);
            
            storage.AddPort(id, Port.Enter());
            storage.AddPort(id, Port.Exit());
            storage.AddPort(id, Port.Exit());

            storage.RemoveNode(id);

            Assert.AreEqual(0, storage.GetPortCount(id));
        }

        [Test]
        public void CreatePortSignatureToIndicesTree() {
            var storage = new BlueprintPortStorage();
            var id = new NodeId(0, 0);
            
            storage.AddPort(id, Port.Enter());
            storage.AddPort(id, Port.Exit());
            storage.AddPort(id, Port.Exit());

            var tree = storage.CreatePortSignatureToIndicesTree(id);

            Assert.IsTrue(tree.ContainsNode(Port.Enter().GetSignature()));
            Assert.IsTrue(tree.ContainsNode(Port.Exit().GetSignature()));
            Assert.AreEqual(2, tree.Roots.Count);

            Assert.IsTrue(tree.TryGetNode(Port.Enter().GetSignature(), out int enterRoot));
            Assert.IsTrue(tree.TryGetNode(Port.Exit().GetSignature(), out int exitRoot));

            Assert.IsTrue(tree.ContainsNode(0, enterRoot));
            Assert.IsTrue(tree.ContainsNode(1, exitRoot));
            Assert.IsTrue(tree.ContainsNode(2, exitRoot));
        }
    }

}
