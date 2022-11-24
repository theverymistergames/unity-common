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
        public void ScheduleWhile_IsCompleting_AfterActionReturnsFalse() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            bool canContinue = true;

            // ReSharper disable once AccessToModifiedClosure
            var job = Jobs.ScheduleWhile(0f, () => canContinue);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();
            timeSource.Run(job);

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            canContinue = false;
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void ScheduleWhileAction_IsNotCalled_AfterCompletion() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var job = Jobs.ScheduleWhile(0f, () => {
                counter++;
                return false;
            });

            timeSource.Initialize(timeProvider);
            timeSource.Enable();
            timeSource.Run(job);

            timeSource.Tick();
            Assert.AreEqual(1, counter);
            Assert.IsTrue(job.IsCompleted);

            timeSource.Tick();
            Assert.AreEqual(1, counter);

            timeSource.Tick();
            Assert.AreEqual(1, counter);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-2)]
        public void ScheduleTimes_IsCompletedAtStart_IfPassedAmountOfTimesIsLessOrEqualZero(int times) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var job = Jobs.ScheduleTimes(0f, 0, () => counter++);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();
            timeSource.Run(job);

            Assert.AreEqual(0, counter);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void ScheduleTimes_IsCalled_PassedAmountOfTimes() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter0 = 0;
            int counter1 = 0;
            int counter2 = 0;

            var job0 = Jobs.ScheduleTimes(0f, 1, () => counter0++);
            var job1 = Jobs.ScheduleTimes(0f, 2, () => counter1++);
            var job2 = Jobs.ScheduleTimes(0f, 4, () => counter2++);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job0);
            timeSource.Run(job1);
            timeSource.Run(job2);

            timeSource.Tick();
            Assert.AreEqual(1, counter0);
            Assert.AreEqual(1, counter1);
            Assert.AreEqual(1, counter2);
            Assert.IsTrue(job0.IsCompleted);
            Assert.IsTrue(!job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);

            timeSource.Tick();
            Assert.AreEqual(2, counter1);
            Assert.AreEqual(2, counter2);
            Assert.IsTrue(job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);

            timeSource.Tick();
            Assert.AreEqual(3, counter2);
            Assert.IsTrue(!job2.IsCompleted);

            timeSource.Tick();
            Assert.AreEqual(4, counter2);
            Assert.IsTrue(job2.IsCompleted);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-2)]
        public void ScheduleTimesWhile_IsCompletedAtStart_IfPassedAmountOfTimesIsLessOrEqualZero(int times) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var job = Jobs.ScheduleTimesWhile(0f, times, () => {
                counter++;
                return true;
            });

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.AreEqual(0, counter);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void ScheduleTimesWhile_IsCompleting_IfActionReturnsFalse() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.ScheduleTimesWhile(0f, 3, () => false);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void ScheduleTimesWhile_IsCompleting_IfExceededTimes() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.ScheduleTimesWhile(0f, 3, () => true);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);

            timeSource.Tick();
            timeSource.Tick();
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
        }
    }

}
