using MisterGames.Blueprints.Core2;
using NUnit.Framework;

namespace Core {

    public class BlueprintFactoryStorageTests {

        [Test]
        public void AddFactories() {
            var storage = new BlueprintFactorySource();

            int factoryId0 = storage.GetOrCreateFactory(typeof(BlueprintFactoryTest0));
            int factoryId1 = storage.GetOrCreateFactory(typeof(BlueprintFactoryTest1));

            Assert.AreEqual(1, factoryId0);
            Assert.AreEqual(2, factoryId1);

            Assert.IsNotNull(storage.GetFactory(factoryId0));
            Assert.IsNotNull(storage.GetFactory(factoryId1));
        }

        [Test]
        public void AddFactoriesWithSameType() {
            var storage = new BlueprintFactorySource();

            int factoryId0 = storage.GetOrCreateFactory(typeof(BlueprintFactoryTest0));
            int factoryId1 = storage.GetOrCreateFactory(typeof(BlueprintFactoryTest0));

            Assert.AreEqual(factoryId0, factoryId1);
        }

        [Test]
        public void RemoveFactory() {
            var storage = new BlueprintFactorySource();

            int factoryId0 = storage.GetOrCreateFactory(typeof(BlueprintFactoryTest0));
            int factoryId1 = storage.GetOrCreateFactory(typeof(BlueprintFactoryTest1));

            storage.RemoveFactory(factoryId0);
            storage.RemoveFactory(factoryId1);

            Assert.IsNull(storage.GetFactory(factoryId0));
            Assert.IsNull(storage.GetFactory(factoryId1));
        }
    }

}
