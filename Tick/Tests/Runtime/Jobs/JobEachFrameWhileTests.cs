using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobEachFrameWhileTests {

        [SetUp]
        public void BeforeEachTest() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var timeSourceProvider = new TimeSourceProvider(stage => timeSource);

            var jobSystem = new JobSystemEachFrameWhile();
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
            var jobSystem = JobSystems.Get<JobSystemEachFrameWhile>();
            var timeSource = TimeSources.Get(PlayerLoopStage.Update);

            jobSystem.DeInitialize();
            timeSource.Unsubscribe(jobSystem);

            JobSystems.InjectProvider(null);
            TimeSources.InjectProvider(null);
        }

        [Test]
        public void EachFrameWhile_IsNotCalled_AtStart() {
            int count = 0;

            var job = Jobs.EachFrameWhile(dt => count++ < 0);
            Assert.AreEqual(0, count);

            job.Start();
            Assert.AreEqual(0, count);
        }

        [Test]
        public void EachFrameWhile_IsCalled_InUpdateLoop() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            int count = 0;
            Jobs.EachFrameWhile(dt => count++ < 5).Start();

            timeSource.Tick();
            Assert.AreEqual(1, count);

            timeSource.Tick();
            Assert.AreEqual(2, count);
        }

        [Test]
        public void EachFrameWhile_IsCalled_InUpdateLoop_UntilWhileReturnsFalse() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            int count = 0;
            var job = Jobs
                .EachFrameWhile(dt => ++count < 3)
                .Start();

            timeSource.Tick();
            Assert.AreEqual(1, count);
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.AreEqual(2, count);
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.AreEqual(3, count);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void EachFrameWhile_CanBeStopped() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            int count = 0;
            var job = Jobs.EachFrameWhile(dt => count++ < 5).Start();

            timeSource.Tick();
            Assert.AreEqual(1, count);

            job.Stop();
            timeSource.Tick();
            Assert.AreEqual(1, count);

            timeSource.Tick();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void EachFrame_CanBeStopped_ThenCanBeContinued() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            int count = 0;
            var job = Jobs.EachFrameWhile(dt => count++ < 5).Start();

            timeSource.Tick();
            Assert.AreEqual(1, count);

            job.Stop();
            timeSource.Tick();
            Assert.AreEqual(1, count);

            job.Start();
            timeSource.Tick();
            Assert.AreEqual(2, count);
        }
    }

}
