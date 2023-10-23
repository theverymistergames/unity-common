using MisterGames.Blueprints.Core2;
using NUnit.Framework;

namespace Core {

    public class RuntimeBlueprintTests {

        [Test]
        public void GetLinks() {
            var factory = new BlueprintFactory();

            int sourceId = factory.GetOrCreateSource(typeof(BlueprintSourceTest3));
            var source = factory.GetSource(sourceId);

            int nodeId0 = source.AddNode();
            int nodeId1 = source.AddNode();
            int nodeId2 = source.AddNode();
            int nodeId3 = source.AddNode();

            var id0 = new NodeId(sourceId, nodeId0);
            var id1 = new NodeId(sourceId, nodeId1);
            var id2 = new NodeId(sourceId, nodeId2);
            var id3 = new NodeId(sourceId, nodeId3);

            var nodeStorage = new RuntimeNodeStorage();
            nodeStorage.AllocateSpace(4);

            nodeStorage.AddNode(id0);
            nodeStorage.AddNode(id1);
            nodeStorage.AddNode(id2);
            nodeStorage.AddNode(id3);

            source.OnSetDefaults(null, id0);
            source.OnSetDefaults(null, id1);
            source.OnSetDefaults(null, id2);
            source.OnSetDefaults(null, id3);

            ref var node = ref source.GetNode<BlueprintNodeTest3>(id0.node);
            node.pickedPort = id0.node;

            node = ref source.GetNode<BlueprintNodeTest3>(id1.node);
            node.pickedPort = id1.node;

            node = ref source.GetNode<BlueprintNodeTest3>(id2.node);
            node.pickedPort = id2.node;

            node = ref source.GetNode<BlueprintNodeTest3>(id3.node);
            node.pickedPort = id3.node;

            var linkStorage = new RuntimeLinkStorage(4, 3, 4);

            int i = linkStorage.SelectPort(id0.source, id0.node, 1);
            i = linkStorage.InsertLinkAfter(i, id1.source, id1.node, 0);
            i = linkStorage.InsertLinkAfter(i, id2.source, id2.node, 0);

            i = linkStorage.SelectPort(id1.source, id1.node, 1);
            i = linkStorage.InsertLinkAfter(i, id3.source, id3.node, 0);

            i = linkStorage.SelectPort(id2.source, id2.node, 1);
            i = linkStorage.InsertLinkAfter(i, id3.source, id3.node, 0);

            var blueprint = new RuntimeBlueprint2(factory, nodeStorage, linkStorage);

            var links = blueprint.GetLinks(id0, 1);

            Assert.IsTrue(links.MoveNext());
            Assert.AreEqual(id1.node, links.Read<int>());

            Assert.IsTrue(links.MoveNext());
            Assert.AreEqual(id2.node, links.Read<int>());

            Assert.IsFalse(links.MoveNext());

            links = blueprint.GetLinks(id1, 1);

            Assert.IsTrue(links.MoveNext());
            Assert.AreEqual(id3.node, links.Read<int>());
            Assert.IsFalse(links.MoveNext());

            links = blueprint.GetLinks(id2, 1);

            Assert.IsTrue(links.MoveNext());
            Assert.AreEqual(id3.node, links.Read<int>());
            Assert.IsFalse(links.MoveNext());

            links = blueprint.GetLinks(id3, 1);
            Assert.IsFalse(links.MoveNext());
        }

        [Test]
        public void EnterNode() {
            var factory = new BlueprintFactory();

            int sourceId = factory.GetOrCreateSource(typeof(BlueprintSourceTest3));
            var source = factory.GetSource(sourceId);

            int nodeId0 = source.AddNode();
            int nodeId1 = source.AddNode();
            int nodeId2 = source.AddNode();

            var id0 = new NodeId(sourceId, nodeId0);
            var id1 = new NodeId(sourceId, nodeId1);
            var id2 = new NodeId(sourceId, nodeId2);

            var nodeStorage = new RuntimeNodeStorage();
            nodeStorage.AllocateSpace(3);

            nodeStorage.AddNode(id0);
            nodeStorage.AddNode(id1);
            nodeStorage.AddNode(id2);

            source.OnSetDefaults(null, id0);
            source.OnSetDefaults(null, id1);
            source.OnSetDefaults(null, id2);

            ref var node = ref source.GetNode<BlueprintNodeTest3>(id0.node);
            Assert.AreEqual(-1, node.pickedPort);

            node = ref source.GetNode<BlueprintNodeTest3>(id1.node);
            Assert.AreEqual(-1, node.pickedPort);

            node = ref source.GetNode<BlueprintNodeTest3>(id2.node);
            Assert.AreEqual(-1, node.pickedPort);

            var linkStorage = new RuntimeLinkStorage(3, 1, 2);

            int i = linkStorage.SelectPort(id0.source, id0.node, 1);
            i = linkStorage.InsertLinkAfter(i, id1.source, id1.node, 0);
            i = linkStorage.InsertLinkAfter(i, id2.source, id2.node, 0);

            var blueprint = new RuntimeBlueprint2(factory, nodeStorage, linkStorage);

            blueprint.Call(id0, 1);

            node = ref source.GetNode<BlueprintNodeTest3>(id1.node);
            Assert.AreEqual(0, node.pickedPort);

            node = ref source.GetNode<BlueprintNodeTest3>(id2.node);
            Assert.AreEqual(0, node.pickedPort);
        }

        [Test]
        public void ReadNode() {
            var factory = new BlueprintFactory();

            int sourceId = factory.GetOrCreateSource(typeof(BlueprintSourceTest3));
            var source = factory.GetSource(sourceId);

            int nodeId0 = source.AddNode();
            int nodeId1 = source.AddNode();

            var id0 = new NodeId(sourceId, nodeId0);
            var id1 = new NodeId(sourceId, nodeId1);

            var nodeStorage = new RuntimeNodeStorage();
            nodeStorage.AllocateSpace(2);

            nodeStorage.AddNode(id0);
            nodeStorage.AddNode(id1);

            ref var node = ref source.GetNode<BlueprintNodeTest3>(id0.node);
            node.pickedPort = id0.node;

            node = ref source.GetNode<BlueprintNodeTest3>(id1.node);
            node.pickedPort = id1.node;

            var linkStorage = new RuntimeLinkStorage(2, 1, 1);

            int i = linkStorage.SelectPort(id0.source, id0.node, 1);
            i = linkStorage.InsertLinkAfter(i, id1.source, id1.node, 0);

            var blueprint = new RuntimeBlueprint2(factory, nodeStorage, linkStorage);

            int value = blueprint.Read<int>(id0, 1);
            Assert.AreEqual(id1.node, value);
        }
    }

}
