using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobObserverTests {

        [SetUp]
        public void BeforeEachTest() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var timeSourceProvider = new TimeSourceProvider(stage => timeSource);

            var jobSystemObserver = new JobSystemObserver();
            var jobSystemAction = new JobSystemAction();
            var jobSystemDelay = new JobSystemDelay();

            var jobIdFactory = new NextJobIdFactory();
            jobSystemObserver.Initialize(jobIdFactory);
            jobSystemDelay.Initialize(jobIdFactory);
            jobSystemAction.Initialize(jobIdFactory);

            timeSource.Subscribe(jobSystemDelay);

            var jobSystemProvider = new JobSystemProvider(getJobSystem: type => {
                if (type == typeof(JobSystemDelay)) return jobSystemDelay;
                if (type == typeof(JobSystemAction)) return jobSystemAction;
                if (type == typeof(JobSystemObserver)) return jobSystemObserver;
                return null;
            });

            var jobSystemProviders = new JobSystemProviders(stage => jobSystemProvider, () => jobSystemProvider);

            TimeSources.InjectProvider(timeSourceProvider);
            JobSystems.InjectProvider(jobSystemProviders);
        }

        [TearDown]
        public void AfterEachTest() {
            var timeSource = TimeSources.Get(PlayerLoopStage.Update);

            var jobSystemDelay = JobSystems.Get<JobSystemDelay>();
            var jobSystemAction = JobSystems.Get<JobSystemAction>();
            var jobSystemObserver = JobSystems.Get<JobSystemObserver>();

            jobSystemDelay.DeInitialize();
            jobSystemAction.DeInitialize();
            jobSystemObserver.DeInitialize();

            timeSource.Unsubscribe(jobSystemDelay);

            JobSystems.InjectProvider(null);
            TimeSources.InjectProvider(null);
        }

        [Test]
        public void ObserverCompleted_AfterWaitForCompleted_SeveralJobs() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            var waitFor0 = Jobs.Action(() => { });
            var waitFor1 = Jobs.Delay(2f);

            var job = JobObserver.Create()
                .Observe(waitFor0)
                .Observe(waitFor1)
                .Push();

            Assert.IsTrue(!waitFor0.IsCompleted);
            Assert.IsTrue(!waitFor1.IsCompleted);
            Assert.IsTrue(!job.IsCompleted);

            waitFor0.Start();
            Assert.IsTrue(waitFor0.IsCompleted);
            Assert.IsTrue(!waitFor1.IsCompleted);
            Assert.IsTrue(!job.IsCompleted);

            waitFor1.Start();
            Assert.IsTrue(!waitFor1.IsCompleted);
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!waitFor1.IsCompleted);
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(waitFor1.IsCompleted);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void ObserverCompleted_AfterWaitForCompleted_AtStart() {
            var waitFor = Jobs.Action(() => { });
            var job = JobObserver.Create()
                .Observe(waitFor)
                .Push();

            Assert.IsTrue(!waitFor.IsCompleted);
            Assert.IsTrue(!job.IsCompleted);

            waitFor.Start();
            Assert.IsTrue(waitFor.IsCompleted);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void WaitCompleted_AfterWaitForCompleted_InUpdateLoop() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            var waitFor = Jobs.Delay(2f);
            var job = JobObserver.Create()
                .Observe(waitFor)
                .Push();

            Assert.IsTrue(!waitFor.IsCompleted);
            Assert.IsTrue(!job.IsCompleted);

            waitFor.Start();
            Assert.IsTrue(!waitFor.IsCompleted);
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!waitFor.IsCompleted);
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(waitFor.IsCompleted);
            Assert.IsTrue(job.IsCompleted);
        }
    }

}
