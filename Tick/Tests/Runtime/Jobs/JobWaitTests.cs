using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobWaitTests {

        [SetUp]
        public void BeforeEachTest() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var timeSourceProvider = new TimeSourceProvider(stage => timeSource);

            var jobSystemWait = new JobSystemWait();
            var jobSystemAction = new JobSystemAction();
            var jobSystemDelay = new JobSystemDelay();

            var jobIdFactory = new NextJobIdFactory();
            jobSystemWait.Initialize(jobIdFactory);
            jobSystemDelay.Initialize(jobIdFactory);
            jobSystemAction.Initialize(jobIdFactory);

            timeSource.Subscribe(jobSystemDelay);

            var jobSystemProvider = new JobSystemProvider(getJobSystem: type => {
                if (type == typeof(JobSystemDelay)) return jobSystemDelay;
                if (type == typeof(JobSystemAction)) return jobSystemAction;
                if (type == typeof(JobSystemWait)) return jobSystemWait;
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
            var jobSystemWait = JobSystems.Get<JobSystemWait>();

            jobSystemDelay.DeInitialize();
            jobSystemAction.DeInitialize();
            jobSystemWait.DeInitialize();

            timeSource.Unsubscribe(jobSystemDelay);

            JobSystems.InjectProvider(null);
            TimeSources.InjectProvider(null);
        }

        [Test]
        public void WaitCompleted_AfterWaitForCompleted_AtStart() {
            var waitFor = Jobs.Action(() => { });
            var job = Jobs.Wait(waitFor);

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
            var job = Jobs.Wait(waitFor);

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
