using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobWaitFramesTests {

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void WaitFrames_IsCompleting_AtStart_IfLessOrEqualsZero(int frames) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.WaitFrames(frames);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public void WaitFrames_IsNotCompleting_AtStart_IfAboveZero(int frames) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.WaitFrames(frames);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void WaitFrames_IsCompleting_InUpdateLoop() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job0 = Jobs.WaitFrames(1);
            var job1 = Jobs.WaitFrames(2);
            var job2 = Jobs.WaitFrames(4);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job0);
            timeSource.Run(job1);
            timeSource.Run(job2);

            timeSource.Tick();
            Assert.IsTrue(job0.IsCompleted);
            Assert.IsTrue(!job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!job2.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job2.IsCompleted);
        }

        [Test]
        public void WaitFrames_CanBeStopped() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.WaitFrames(2);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            job.Stop();
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void WaitFrames_CanBeStopped_ThenCanBeContinued() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.WaitFrames(2);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            job.Stop();
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            job.Start();
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
        }
    }

}
