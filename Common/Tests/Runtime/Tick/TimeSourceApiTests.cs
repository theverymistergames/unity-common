using MisterGames.Common.Tick;
using NUnit.Framework;
using Utils;

namespace Core {

    public class TimeSourceApiTests {

        [SetUp]
        public void BeforeEachTest() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var timeSourceProvider = new TimeSourceProvider(stage => timeSource);

            TimeSources.InjectProvider(timeSourceProvider);
        }

        [TearDown]
        public void AfterEachTest() {
            TimeSources.InjectProvider(null);
        }

        [Test]
        public void Test_ApiCalls_Sequentially() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            Assert.AreEqual(0f, timeSource.DeltaTime);

            timeSource.Tick();
            Assert.AreEqual(1f, timeSource.DeltaTime);

            timeSource.Tick();
            Assert.AreEqual(1f, timeSource.DeltaTime);

            timeSource.Reset();
            Assert.AreEqual(0f, timeSource.DeltaTime);
        }
    }

}
