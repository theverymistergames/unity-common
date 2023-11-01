using MisterGames.Blueprints;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Nodes;
using MisterGames.Blueprints.Runtime;
using MisterGames.Common.Data;
using NUnit.Framework;
using UnityEngine;

namespace Core {

    public class BlueprintCompilationTests {

        private class TestClass {

            public int value;

            public TestClass(int value) {
                this.value = value;
            }

            public override string ToString() {
                return $"{nameof(TestClass)}({value}, hash {GetHashCode()})";
            }
        }

        [Test]
        public void CreateNode() {
            var meta = new BlueprintMeta2 { Owner = this };

            var id0 = meta.AddNode(typeof(BlueprintSourceTest2));
            var source0 = meta.GetNodeSource(id0);

            ref var node = ref source0.GetNode<BlueprintNodeTest2>(id0.node);

            node.intValue = 5;
            node.objectValue = ScriptableObject.CreateInstance<BlueprintAsset2>();
            node.referenceValue = new TestClass(5);

            var factory = new BlueprintFactory();
            BlueprintCompilation.TryCreateNode(factory, meta, id0, out var runtimeId0);
            var runtimeSource0 = factory.GetSource(runtimeId0.source);

            ref var runtimeNode = ref runtimeSource0.GetNode<BlueprintNodeTest2>(runtimeId0.node);

            Assert.AreEqual(node.intValue, runtimeNode.intValue);
            Assert.AreEqual(node.objectValue, runtimeNode.objectValue);

            Assert.AreNotEqual(node.referenceValue, runtimeNode.referenceValue);
            Assert.IsNotNull(runtimeNode.referenceValue);
            Assert.AreEqual(((TestClass) node.referenceValue).value, ((TestClass) runtimeNode.referenceValue).value);
        }

        [Test]
        public void CompileHashLinks() {
            var meta = new BlueprintMeta2 { Owner = this };

            var id0 = meta.AddNode(typeof(BlueprintSourceGoto));
            var id1 = meta.AddNode(typeof(BlueprintSourceGoto));
            var id2 = meta.AddNode(typeof(BlueprintSourceGotoExit));
            var id3 = meta.AddNode(typeof(BlueprintSourceGotoExit));

            var source0 = meta.GetNodeSource(id0);
            var source1 = meta.GetNodeSource(id1);
            var source2 = meta.GetNodeSource(id2);
            var source3 = meta.GetNodeSource(id3);

            var links = new TreeMap<int, RuntimeLink2>();

            BlueprintCompilation.AddHashLink(links, meta, source0, id0, id0);
            BlueprintCompilation.AddHashLink(links, meta, source1, id1, id1);
            BlueprintCompilation.AddHashLink(links, meta, source2, id2, id2);
            BlueprintCompilation.AddHashLink(links, meta, source3, id3, id3);

            var linkStorage = new RuntimeLinkStorage();
            BlueprintCompilation.CompileHashLinks(linkStorage, links);

            int l = linkStorage.GetFirstLink(id0.source, id0.node, 1);
            Assert.IsTrue(l >= 0);

            var link = linkStorage.GetLink(l);
            Assert.AreEqual(id3.source, link.source);
            Assert.AreEqual(id3.node, link.node);

            l = linkStorage.GetNextLink(l);
            Assert.IsTrue(l >= 0);

            link = linkStorage.GetLink(l);
            Assert.AreEqual(id2.source, link.source);
            Assert.AreEqual(id2.node, link.node);

            l = linkStorage.GetFirstLink(id1.source, id1.node, 1);
            Assert.IsTrue(l >= 0);

            link = linkStorage.GetLink(l);
            Assert.AreEqual(id3.source, link.source);
            Assert.AreEqual(id3.node, link.node);

            l = linkStorage.GetNextLink(l);
            Assert.IsTrue(l >= 0);

            link = linkStorage.GetLink(l);
            Assert.AreEqual(id2.source, link.source);
            Assert.AreEqual(id2.node, link.node);
        }

        [Test]
        public void CompileBlueprint() {
            var meta = new BlueprintMeta2 { Owner = this };

            var id0 = meta.AddNode(typeof(BlueprintSourceStart));
            var id1 = meta.AddNode(typeof(BlueprintSourceGoto));
            var id2 = meta.AddNode(typeof(BlueprintSourceGotoExit));
            var id3 = meta.AddNode(typeof(BlueprintSourceGotoExit));
        }
    }

}
