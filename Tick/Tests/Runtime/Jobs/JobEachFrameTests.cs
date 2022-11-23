using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobEachFrameTests {

        [Test]
        public void EachFrameAction_IsCalledEachTick() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var job = Jobs.EachFrame(() => counter++);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(counter == 0);

            timeSource.Tick();
            Assert.IsTrue(counter == 1);

            timeSource.Tick();
            Assert.IsTrue(counter == 2);
        }

        [Test]
        public void EachFrameAction_CanBeStopped() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var job = Jobs.EachFrame(() => counter++);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.IsTrue(counter == 1);

            job.Stop();
            timeSource.Tick();
            Assert.IsTrue(counter == 1);

            timeSource.Tick();
            Assert.IsTrue(counter == 1);
        }

        [Test]
        public void EachFrameAction_CanBeStarted_AfterStop() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var job = Jobs.EachFrame(() => counter++);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.IsTrue(counter == 1);

            job.Stop();
            timeSource.Tick();
            timeSource.Tick();
            Assert.IsTrue(counter == 1);

            job.Start();
            timeSource.Tick();
            Assert.IsTrue(counter == 2);

            timeSource.Tick();
            Assert.IsTrue(counter == 3);
        }

        [Test]
        public void EachFrameWhile_IsCompleting_AfterActionWhileReturnsFalse() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            bool canContinue = true;

            // ReSharper disable once AccessToModifiedClosure
            var job = Jobs.EachFrameWhile(() => canContinue);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            canContinue = false;
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
        }
    }

}
