using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobProcessTests {

        [Test]
        public void Process_IsNotCompleted_AtStart_IfGetProcessReturnsOne() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.EachFrameProcess(() => 1f, _ => { });

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void Process_IsCompleted_InUpdateLoop_IfGetProcessReturnsOne() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.EachFrameProcess(() => 1f, _ => { });

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void Process_IsNotCompleting_UntilProcessValueReachesOne() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            float processIncrementPerTick = 0f;
            float processValue = 0f;

            // ReSharper disable once AccessToModifiedClosure
            var job = Jobs.EachFrameProcess(() => processValue + processIncrementPerTick, process => processValue = process);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            processIncrementPerTick = 0.4f;
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        [TestCase(-2f)]
        [TestCase(-1f)]
        [TestCase(-0.5f)]
        [TestCase(0f)]
        [TestCase(0.5f)]
        [TestCase(1f)]
        [TestCase(2f)]
        public void ProcessValue_IsInRange_BetweenOneAndZero_Inclusive(float process) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            float processValue = 0f;
            var job = Jobs.EachFrameProcess(() => process, result => processValue = result);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();

            if (process <= 0f) {
                Assert.IsTrue(processValue.IsNearlyZero());
                return;
            }

            if (process >= 1f) {
                Assert.IsTrue(processValue.IsNearlyEqual(1f));
                return;
            }

            Assert.IsTrue(processValue.IsNearlyEqual(process));
        }
    }

}
