using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobDelayTests {

        [Test]
        [TestCase(0f)]
        [TestCase(-1f)]
        public void Delay_IsCompleting_AtStart_IfLessOrEqualsZero(float delaySeconds) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.Delay(delaySeconds);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        [TestCase(1f)]
        [TestCase(2f)]
        public void Delay_IsNotCompleting_AtStart_IfAboveZero(float delaySeconds) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.Delay(delaySeconds);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void Delay_IsCompleting_InUpdateLoop() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job0 = Jobs.Delay(1f);
            var job1 = Jobs.Delay(2f);
            var job2 = Jobs.Delay(4f);

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
    }

}
