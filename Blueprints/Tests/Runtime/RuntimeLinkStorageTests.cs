using MisterGames.Blueprints.Runtime;
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
        public void InlineLinks_Link_Link() {
            var storage = new RuntimeLinkStorage();

            int i0 = storage.SelectPort(0, 0, 0);
            int i1 = storage.InsertLinkAfter(i0, 0, 0, 1);

            int i2 = storage.SelectPort( 0, 0, 1);
            int i3 = storage.InsertLinkAfter(i2, 0, 0, 2);

            storage.InlineLinks();

            int l = storage.GetFirstLink(0, 0, 0);
            Assert.IsTrue(l >= 0);
            Assert.AreEqual(2, storage.GetLink(l).port);

            Assert.IsFalse(storage.GetNextLink(l) >= 0);

            l = storage.GetFirstLink(0, 0, 1);
            Assert.IsFalse(l >= 0);
        }

        [Test]
        public void InlineLinks_Link_TwoLinks() {
            var storage = new RuntimeLinkStorage();

            int i0 = storage.SelectPort(0, 0, 0);
            int i1 = storage.InsertLinkAfter(i0, 0, 0, 1);
            int i1_1 = storage.InsertLinkAfter(i1, 0, 0, 3);

            int i2 = storage.SelectPort( 0, 0, 1);
            int i3 = storage.InsertLinkAfter(i2, 0, 0, 2);

            storage.InlineLinks();

            int l = storage.GetFirstLink(0, 0, 0);
            Assert.IsTrue(l >= 0);
            Assert.AreEqual(2, storage.GetLink(l).port);

            l = storage.GetNextLink(l);
            Assert.IsTrue(l >= 0);
            Assert.AreEqual(3, storage.GetLink(l).port);

            Assert.IsFalse(storage.GetNextLink(l) >= 0);
        }

        [Test]
        public void InlineLinks_TwoLinks_Link() {
            var storage = new RuntimeLinkStorage();

            int i0 = storage.SelectPort(0, 0, 0);
            int i1 = storage.InsertLinkAfter(i0, 0, 0, 1);
            int i1_1 = storage.InsertLinkAfter(i1, 0, 0, 2);

            int i2 = storage.SelectPort( 0, 0, 2);
            int i3 = storage.InsertLinkAfter(i2, 0, 0, 3);

            storage.InlineLinks();

            int l = storage.GetFirstLink(0, 0, 0);
            Assert.IsTrue(l >= 0);
            Assert.AreEqual(1, storage.GetLink(l).port);

            l = storage.GetNextLink(l);
            Assert.IsTrue(l >= 0);
            Assert.AreEqual(3, storage.GetLink(l).port);

            Assert.IsFalse(storage.GetNextLink(l) >= 0);
        }

        [Test]
        public void InlineLinks_Many() {
            var storage = new RuntimeLinkStorage();

            int i0 = storage.SelectPort(0, 0, 0);
            int i1 = storage.InsertLinkAfter(i0, 0, 0, 1);
            int i1_1 = storage.InsertLinkAfter(i1, 0, 0, 3);
            int i1_2 = storage.InsertLinkAfter(i1_1, 0, 0, 5);

            int i2 = storage.SelectPort( 0, 0, 1);
            int i3 = storage.InsertLinkAfter(i2, 0, 0, 2);

            int i4 = storage.SelectPort( 0, 0, 3);
            int i5 = storage.InsertLinkAfter(i4, 0, 0, 4);
            int i6 = storage.InsertLinkAfter(i5, 0, 0, 6);

            int i7 = storage.SelectPort( 0, 0, 6);
            int i8 = storage.InsertLinkAfter(i7, 0, 0, 7);

            storage.InlineLinks();

            int l = storage.GetFirstLink(0, 0, 0);
            Assert.IsTrue(l >= 0);
            Assert.AreEqual(2, storage.GetLink(l).port);

            l = storage.GetNextLink(l);
            Assert.IsTrue(l >= 0);
            Assert.AreEqual(4, storage.GetLink(l).port);

            l = storage.GetNextLink(l);
            Assert.IsTrue(l >= 0);
            Assert.AreEqual(7, storage.GetLink(l).port);

            l = storage.GetNextLink(l);
            Assert.IsTrue(l >= 0);
            Assert.AreEqual(5, storage.GetLink(l).port);

            Assert.IsFalse(storage.GetNextLink(l) >= 0);
        }
    }

}
