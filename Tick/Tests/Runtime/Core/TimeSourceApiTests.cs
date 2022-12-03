using MisterGames.Tick.Core;
using NUnit.Framework;
using Utils;

namespace Core {

    public class TimeSourceApiTests {

        [Test]
        public void Test_ApiCalls_Sequentially() {
            var timeProvider = new ConstantTimeProvider(1f);
            var timeSource = new TimeSource(timeProvider);
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
