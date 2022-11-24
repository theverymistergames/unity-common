using MisterGames.Tick.Core;
using NUnit.Framework;
using Utils;

namespace Core {

    public class TimeSourceTests {

        [Test]
        public void TimeScale_Changes_AtEndOfTick() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Tick();
            Assert.AreEqual(1f, timeSource.DeltaTime);

            timeSource.TimeScale = 2f;
            Assert.AreEqual(1f, timeSource.DeltaTime);

            timeSource.Tick();
            Assert.AreEqual(2f, timeSource.DeltaTime);
        }

        [Test]
        public void Can_ChangeTimeScale_InUpdateLoop_And_WillBeChanged_AtEndOfTick() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var changeTimeScaleOnUpdate = new ActionOnUpdate(update => timeSource.TimeScale = 2f);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Tick();
            Assert.AreEqual(1f, timeSource.DeltaTime);

            timeSource.Subscribe(changeTimeScaleOnUpdate);
            timeSource.Tick();
            Assert.AreEqual(2f, timeSource.DeltaTime);
        }

        [Test]
        public void Subscribers_AreNotUpdating_IfPaused() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new CountOnUpdate();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);

            timeSource.IsPaused = true;
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);
        }

        [Test]
        public void Subscribers_AreUpdating_After_DisablePause() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new CountOnUpdate();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

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
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new CountOnUpdate();
            var pauseOnUpdate = new ActionOnUpdate(update => timeSource.IsPaused = true);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

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
