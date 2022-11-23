using MisterGames.Common.Maths;
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
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(1f));

            timeSource.TimeScale = 2f;
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(1f));

            timeSource.Tick();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(2f));
        }

        [Test]
        public void Can_ChangeTimeScale_InUpdateLoop_And_WillBeChanged_AtEndOfTick() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var changeTimeScaleOnUpdate = new ActionOnUpdate(update => timeSource.TimeScale = 2f);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Tick();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(1f));

            timeSource.Subscribe(changeTimeScaleOnUpdate);
            timeSource.Tick();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(2f));
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
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.IsPaused = true;
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);
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
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.IsPaused = true;
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.IsPaused = false;
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 2);
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
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.Subscribe(pauseOnUpdate);
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 2);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 2);
        }

    }

}
