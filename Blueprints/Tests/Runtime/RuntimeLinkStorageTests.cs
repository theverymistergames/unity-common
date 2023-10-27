using MisterGames.Blueprints.Core2;
using NUnit.Framework;

namespace Core {

    public class RuntimeLinkStorageTests {

        [Test]
        public void AddFirstLink() {
            var storage = new RuntimeLinkStorage();

            int i = storage.SelectPort(0, 0, 0);
            i = storage.InsertLinkAfter(i, 0, 0, 1);

            int first = storage.GetFirstLink(0, 0, 0);
            Assert.IsTrue(first >= 0);

            var link = storage.GetLink(first);
            Assert.AreEqual(1, link.port);
        }

        [Test]
        public void AddTwoLinks() {
            var storage = new RuntimeLinkStorage();

            int i = storage.SelectPort(0, 0, 0);
            i = storage.InsertLinkAfter(i, 0, 0, 1);
            i = storage.InsertLinkAfter(i, 0, 0, 2);

            int first = storage.GetFirstLink(0, 0, 0);
            int index = storage.GetNextLink(first);
            Assert.IsTrue(index >= 0);

            var link = storage.GetLink(index);
            Assert.AreEqual(2, link.port);
        }

        [Test]
        public void AddTwoPorts() {
            var storage = new RuntimeLinkStorage();

            int i = storage.SelectPort(0, 0, 0);
            i = storage.InsertLinkAfter(i, 0, 0, 1);
            i = storage.InsertLinkAfter(i, 0, 0, 2);

            i = storage.SelectPort(0, 0, 3);
            i = storage.InsertLinkAfter(i, 0, 0, 4);
            i = storage.InsertLinkAfter(i, 0, 0, 5);

            int index = storage.GetFirstLink(0, 0, 3);
            var link = storage.GetLink(index);
            Assert.AreEqual(4, link.port);

            index = storage.GetNextLink(index);
            link = storage.GetLink(index);
            Assert.AreEqual(5, link.port);
        }

        [Test]
        public void RemoveLinks() {
            var storage = new RuntimeLinkStorage();

            int i0 = storage.SelectPort(0, 0, 0);
            int i1 = storage.InsertLinkAfter(i0, 0, 0, 1);
            int i2 = storage.InsertLinkAfter(i1, 0, 0, 2);
            int i3 = storage.InsertLinkAfter(i2, 0, 0, 3);
            int i4 = storage.InsertLinkAfter(i3, 0, 0, 4);

            storage.RemoveLink(0, 0, 3);
            storage.RemoveLink(0, 0, 1);

            int index = storage.GetFirstLink(0, 0, 0);
            Assert.IsTrue(index >= 0);

            var link = storage.GetLink(index);
            Assert.AreEqual(2, link.port);

            index = storage.GetNextLink(index);
            Assert.IsTrue(index >= 0);

            link = storage.GetLink(index);
            Assert.AreEqual(4, link.port);
        }

        [Test]
        public void InlineLinks() {
            var storage = new RuntimeLinkStorage();

            int i0 = storage.SelectPort(0, 0, 0);
            int i1 = storage.InsertLinkAfter(i0, 0, 0, 1);

            int i2 = storage.SelectPort( 0, 0, 2);
            int i3 = storage.InsertLinkAfter(i2, 0, 0, 3);


        }
    }

}
