using MisterGames.Tick.Core;
using NUnit.Framework;
using Utils;

namespace Core {

    public class TimeSourceTests {

        [Test]
        public void TimeScale_Changes_AtEndOfTick() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());

            timeSource.Tick();
            Assert.AreEqual(1f, timeSource.DeltaTime);

            timeSource.TimeScale = 2f;
            Assert.AreEqual(1f, timeSource.DeltaTime);

            timeSource.Tick();
            Assert.AreEqual(2f, timeSource.DeltaTime);
        }

        [Test]
        public void Can_ChangeTimeScale_InUpdateLoop_And_WillBeChanged_AtEndOfTick() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var changeTimeScaleOnUpdate = new ActionOnUpdate(update => timeSource.TimeScale = 2f);

            timeSource.Tick();
            Assert.AreEqual(1f, timeSource.DeltaTime);

            timeSource.Subscribe(changeTimeScaleOnUpdate);
            timeSource.Tick();
            Assert.AreEqual(2f, timeSource.DeltaTime);
        }

        [Test]
        public void Subscribers_AreNotUpdating_IfPaused() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var frameCounter = new CountOnUpdate();

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);

            timeSource.IsPaused = true;
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);
        }

        [Test]
        public void Subscribers_AreUpdating_After_DisablePause() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var frameCounter = new CountOnUpdate();

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);

            timeSource.IsPaused = true;
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);

            timeSource.IsPaused = false;
            timeSource.Tick();
            Assert.AreEqual(2, frameCounter.Count);
        }

        [Test]
        public void Can_Pause_InUpdateLoop_And_WillBePaused_OnNextTick() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var frameCounter = new CountOnUpdate();
            var pauseOnUpdate = new ActionOnUpdate(update => timeSource.IsPaused = true);

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);

            timeSource.Subscribe(pauseOnUpdate);
            timeSource.Tick();
            Assert.AreEqual(2, frameCounter.Count);

            timeSource.Tick();
            Assert.AreEqual(2, frameCounter.Count);
        }
    }

}
