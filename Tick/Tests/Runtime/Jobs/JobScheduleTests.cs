using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobScheduleTests {

        [Test]
        public void ScheduleAction_IsNotCalled_AtStart() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            bool called = false;
            var job = Jobs.Schedule(0f, () => called = true);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!called);
        }

        [Test]
        public void ScheduleAction_IsCalled_OnNextTick_IfHasZeroPeriod() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            bool called = false;
            var job = Jobs.Schedule(0f, () => called = true);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();
            timeSource.Run(job);

            timeSource.Tick();
            Assert.IsTrue(called);
        }

        [Test]
        public void ScheduleAction_IsCalled_OnPassedPeriod() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var job = Jobs.Schedule(2f, () => counter++);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();
            timeSource.Run(job);

            timeSource.Tick();
            Assert.AreEqual(0, counter);

            timeSource.Tick();
            Assert.AreEqual(1, counter);

            timeSource.Tick();
            Assert.AreEqual(1, counter);

            timeSource.Tick();
            Assert.AreEqual(2, counter);

            timeSource.Tick();
            Assert.AreEqual(2, counter);
        }

        [Test]
        public void ScheduleWhileAction_IsNotCalled_AfterCompletion() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            bool called = false;
            var job = Jobs.Schedule(0f, () => called = true);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();
            timeSource.Run(job);

            timeSource.Tick();
            Assert.IsTrue(called);
        }
    }

}
