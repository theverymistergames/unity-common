using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobDelayTests {

        private TimeSource _timeSource;

        [SetUp]
        public void BeforeEachTest() {
            var jobSystem = new JobSystemDelay();
            var jobSystemProvider = new JobSystemProvider(getJobSystem: () => jobSystem);
            var jobSystemProviders = new JobSystemProviders(stage => jobSystemProvider, () => jobSystemProvider);

            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var timeSourceProvider = new TimeSourceProvider(stage => timeSource);

            JobSystems.InjectProvider(jobSystemProviders);
            TimeSources.InjectProvider(timeSourceProvider);
        }

        [TearDown]
        public void AfterEachTest() {
            JobSystems.InjectProvider(null);
            TimeSources.InjectProvider(null);
        }

        [Test]
        [TestCase(0f)]
        [TestCase(-1f)]
        public void Delay_IsCompleted_IfLessOrEqualsZero(float delaySeconds) {
            var job = Jobs.Delay(delaySeconds);
            Assert.IsTrue(job.IsCompleted);

            job.Start();
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        [TestCase(1f)]
        [TestCase(2f)]
        public void Delay_IsNotCompleted_AtStart_IfAboveZero(float delaySeconds) {
            var job = Jobs.Delay(delaySeconds);
            Assert.IsTrue(!job.IsCompleted);

            job.Start();
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void Delay_IsCompleting_InUpdateLoop() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            var job0 = Jobs.Delay(1f).Start();
            var job1 = Jobs.Delay(2f).Start();
            var job2 = Jobs.Delay(4f).Start();

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
        public void Delay_CanBeStopped() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var job = Jobs.Delay(2f).Start();

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            job.Stop();
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void Delay_CanBeStopped_ThenCanBeContinued() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var job = Jobs.Delay(2f).Start();

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
