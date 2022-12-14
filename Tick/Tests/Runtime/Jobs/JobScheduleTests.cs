using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobScheduleTests {

        [SetUp]
        public void BeforeEachTest() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var timeSourceProvider = new TimeSourceProvider(stage => timeSource);

            var jobSystem = new JobSystemSchedule();
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
            var jobSystem = JobSystems.Get<JobSystemSchedule>();
            var timeSource = TimeSources.Get(PlayerLoopStage.Update);

            jobSystem.DeInitialize();
            timeSource.Unsubscribe(jobSystem);

            JobSystems.InjectProvider(null);
            TimeSources.InjectProvider(null);
        }

        [Test]
        public void Schedule_IsNotCalled_AtStart() {
            int count = 0;

            var job = Jobs.Schedule(() => count++, 0f);
            Assert.AreEqual(0, count);

            job.Start();
            Assert.AreEqual(0, count);
        }

        [Test]
        public void Schedule_IsCalled_InUpdateLoop() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            int count = 0;
            Jobs.Schedule(() => count++, 0f).Start();

            timeSource.Tick();
            Assert.AreEqual(1, count);

            timeSource.Tick();
            Assert.AreEqual(2, count);
        }

        [Test]
        public void Schedule_IsCompleting_AtStart_IfMaxTimesIsZero() {
            int count = 0;
            var job = Jobs.Schedule(() => count++, 0, 0);

            Assert.AreEqual(0, count);
            Assert.IsTrue(job.IsCompleted);

            job.Start();
            Assert.AreEqual(0, count);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void Schedule_IsCompleting_InUpdateLoop_IfExceededMaxTimes() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            int count = 0;
            var job = Jobs.Schedule(() => count++, 0f, 3).Start();

            timeSource.Tick();
            Assert.AreEqual(1, count);
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.AreEqual(2, count);
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.AreEqual(3, count);
            Assert.IsTrue(job.IsCompleted);

            timeSource.Tick();
            Assert.AreEqual(3, count);
        }

        [Test]
        public void Schedule_CanBeStopped() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            int count = 0;
            var job = Jobs.Schedule(() => count++, 0f).Start();

            timeSource.Tick();
            Assert.AreEqual(1, count);

            job.Stop();
            timeSource.Tick();
            Assert.AreEqual(1, count);

            timeSource.Tick();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void Schedule_CanBeStopped_ThenCanBeContinued() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            int count = 0;
            var job = Jobs.Schedule(() => count++, 0f).Start();

            timeSource.Tick();
            Assert.AreEqual(1, count);

            job.Stop();
            timeSource.Tick();
            Assert.AreEqual(1, count);

            job.Start();
            timeSource.Tick();
            Assert.AreEqual(2, count);
        }

        [Test]
        public void ScheduleAction_CalledOnPeriod() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            int count = 0;
            var job = Jobs.Schedule(() => count++, 2f).Start();

            timeSource.Tick();
            Assert.AreEqual(0, count);

            timeSource.Tick();
            Assert.AreEqual(1, count);

            timeSource.Tick();
            Assert.AreEqual(1, count);

            timeSource.Tick();
            Assert.AreEqual(2, count);
        }
    }

}
