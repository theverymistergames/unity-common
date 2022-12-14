using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobWaitFramesTests {

        [SetUp]
        public void BeforeEachTest() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var timeSourceProvider = new TimeSourceProvider(stage => timeSource);

            var jobSystem = new JobSystemWaitFrames();
            var jobIdFactory = new NextJobIdFactory();
            jobSystem.Initialize(jobIdFactory);
            timeSource.Subscribe(jobSystem);

            var jobSystemProvider = new JobSystemProvider(getJobSystem: type => jobSystem);
            var jobSystemProviders = new JobSystemProviders(stage => jobSystemProvider, () => jobSystemProvider);

            TimeSources.InjectProvider(timeSourceProvider);
            JobSystems.InjectProvider(jobSystemProviders);
        }

        [TearDown]
        public void AfterEachTest() {
            var jobSystem = JobSystems.Get<JobSystemWaitFrames>();
            var timeSource = TimeSources.Get(PlayerLoopStage.Update);

            jobSystem.DeInitialize();
            timeSource.Unsubscribe(jobSystem);

            JobSystems.InjectProvider(null);
            TimeSources.InjectProvider(null);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void WaitFrames_IsCompleted_IfLessOrEqualsZero(int frames) {
            var job = Jobs.WaitFrames(frames);
            Assert.IsTrue(job.IsCompleted);

            job.Start();
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        public void WaitFrames_IsNotCompleted_AtStart_IfAboveZero(int frames) {
            var job = Jobs.WaitFrames(frames);
            Assert.IsTrue(!job.IsCompleted);

            job.Start();
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void WaitFrames_IsCompleting_InUpdateLoop() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            var job0 = Jobs.WaitFrames(1).Start();
            var job1 = Jobs.WaitFrames(2).Start();
            var job2 = Jobs.WaitFrames(4).Start();

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
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var job = Jobs.WaitFrames(2).Start();

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            job.Stop();
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void WaitFrames_CanBeStopped_ThenCanBeContinued() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var job = Jobs.WaitFrames(2).Start();

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
