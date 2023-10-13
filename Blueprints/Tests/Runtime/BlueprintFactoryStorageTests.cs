using MisterGames.Blueprints.Core2;
using NUnit.Framework;

namespace Core {

    public class BlueprintFactoryStorageTests {

        [Test]
        public void AddFactories() {
            var storage = new BlueprintFactory();

            int factoryId0 = storage.GetOrCreateSource(typeof(BlueprintSourceTest0));
            int factoryId1 = storage.GetOrCreateSource(typeof(BlueprintSourceTest1));

            Assert.AreEqual(1, factoryId0);
            Assert.AreEqual(2, factoryId1);

            Assert.IsNotNull(storage.GetSource(factoryId0));
            Assert.IsNotNull(storage.GetSource(factoryId1));
        }

        [Test]
        public void AddFactoriesWithSameType() {
            var storage = new BlueprintFactory();

            int factoryId0 = storage.GetOrCreateSource(typeof(BlueprintSourceTest0));
            int factoryId1 = storage.GetOrCreateSource(typeof(BlueprintSourceTest0));

            Assert.AreEqual(factoryId0, factoryId1);
        }

        [Test]
        public void RemoveFactory() {
            var storage = new BlueprintFactory();

            int factoryId0 = storage.GetOrCreateSource(typeof(BlueprintSourceTest0));
            int factoryId1 = storage.GetOrCreateSource(typeof(BlueprintSourceTest1));

            storage.RemoveSource(factoryId0);
            storage.RemoveSource(factoryId1);

            Assert.IsNull(storage.GetSource(factoryId0));
            Assert.IsNull(storage.GetSource(factoryId1));
        }
    }

}
