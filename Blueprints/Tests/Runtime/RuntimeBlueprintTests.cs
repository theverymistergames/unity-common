using MisterGames.Blueprints.Core2;
using NUnit.Framework;

namespace Core {

    public class RuntimeBlueprintTests {
/*
        [Test]
        public void SetGetLinks() {
            var factory = new BlueprintFactory();

            int sourceId = factory.GetOrCreateSource(typeof(BlueprintSourceTest0));
            var source = factory.GetSource(sourceId);

            int nodeId0 = source.AddNode();
            int nodeId1 = source.AddNode();
            int nodeId2 = source.AddNode();
            int nodeId3 = source.AddNode();

            long id0 = BlueprintNodeAddress.Pack(sourceId, nodeId0);
            long id1 = BlueprintNodeAddress.Pack(sourceId, nodeId1);
            long id2 = BlueprintNodeAddress.Pack(sourceId, nodeId2);
            long id3 = BlueprintNodeAddress.Pack(sourceId, nodeId3);

            var blueprint = new RuntimeBlueprint2(factory, 4, 3, 4);

            blueprint.AddNode(id0);
            blueprint.AddNode(id1);
            blueprint.AddNode(id2);
            blueprint.AddNode(id3);

            blueprint.SetPort(id0, 1, 2);
            blueprint.AddLink(id1, 0);
            blueprint.AddLink(id2, 0);

            blueprint.SetPort(id1, 1, 1);
            blueprint.AddLink(id3, 0);

            blueprint.SetPort(id2, 1, 1);
            blueprint.AddLink(id3, 0);

            blueprint.GetLinks(id0, 1, out int index, out _);

            var link = blueprint.GetLink(index);

            Assert.AreEqual(id1, link.nodeId);
            Assert.AreEqual(0, link.port);

            link = blueprint.GetLink(index + 1);
            Assert.AreEqual(id2, link.nodeId);
            Assert.AreEqual(0, link.port);

            blueprint.GetLinks(id1, 1, out index, out _);

            link = blueprint.GetLink(index);
            Assert.AreEqual(id3, link.nodeId);
            Assert.AreEqual(0, link.port);

            blueprint.GetLinks(id2, 1, out index, out _);

            link = blueprint.GetLink(index);
            Assert.AreEqual(id3, link.nodeId);
            Assert.AreEqual(0, link.port);
        }

        [Test]
        public void EnterNode() {
            var factory = new BlueprintFactory();

            int sourceId = factory.GetOrCreateSource(typeof(BlueprintSourceTest3));
            var source = factory.GetSource(sourceId);

            int nodeId0 = source.AddNode();
            int nodeId1 = source.AddNode();

            long id0 = BlueprintNodeAddress.Pack(sourceId, nodeId0);
            long id1 = BlueprintNodeAddress.Pack(sourceId, nodeId1);

            source.OnSetDefaults(id0);
            source.OnSetDefaults(id1);

            ref var node0 = ref source.GetNode<BlueprintNodeTest3>(nodeId0);
            ref var node1 = ref source.GetNode<BlueprintNodeTest3>(nodeId1);

            Assert.AreEqual(-1, node0.pickedPort);
            Assert.AreEqual(-1, node1.pickedPort);

            var blueprint = new RuntimeBlueprint2(factory, 2, 1, 1);

            blueprint.AddNode(id0);
            blueprint.AddNode(id1);

            blueprint.SetPort(id0, 1, 1);
            blueprint.AddLink(id1, 0);

            blueprint.Call(id0, 1);

            Assert.AreEqual(0, node1.pickedPort);
        }

        [Test]
        public void ReadNode() {
            var factory = new BlueprintFactory();

            int sourceId = factory.GetOrCreateSource(typeof(BlueprintSourceTest3));
            var source = factory.GetSource(sourceId);

            int nodeId0 = source.AddNode();
            int nodeId1 = source.AddNode();

            long id0 = BlueprintNodeAddress.Pack(sourceId, nodeId0);
            long id1 = BlueprintNodeAddress.Pack(sourceId, nodeId1);

            source.OnSetDefaults(id1);

            var blueprint = new RuntimeBlueprint2(factory, 2, 1, 1);

            blueprint.AddNode(id0);
            blueprint.AddNode(id1);

            blueprint.SetPort(id0, 1, 1);
            blueprint.AddLink(id1, 0);

            int port = blueprint.Read<int>(id0, 1);
            Assert.AreEqual(-1, port);
        }
        */
    }

}
