using MisterGames.Blueprints.Core2;
using NUnit.Framework;

namespace Core {

    public class BlueprintNodeAddressTests {

        [Test]
        [TestCase(0, 1)]
        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(1, 1)]
        [TestCase(0, -1)]
        [TestCase(-1, 0)]
        [TestCase(-1, -1)]
        [TestCase(10, 10)]
        [TestCase(10, -10)]
        [TestCase(-10, 10)]
        [TestCase(-10, -10)]
        [TestCase(int.MaxValue, int.MaxValue)]
        [TestCase(int.MinValue, int.MinValue)]
        [TestCase(int.MaxValue, int.MinValue)]
        [TestCase(int.MinValue, int.MaxValue)]
        public void PackUnpack(int id0, int id1) {
            long id = BlueprintNodeAddress.Pack(id0, id1);
            BlueprintNodeAddress.Unpack(id, out int actualId0, out int actualId1);

            Assert.AreEqual(id0, actualId0);
            Assert.AreEqual(id1, actualId1);
        }
    }

}
