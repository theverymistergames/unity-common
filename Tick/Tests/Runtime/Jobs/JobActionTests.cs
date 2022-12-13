using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobActionTests {

        [SetUp]
        public void BeforeEachTest() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var timeSourceProvider = new TimeSourceProvider(stage => timeSource);

            var jobSystem = new JobSystemAction();
            var jobIdFactory = new NextJobIdFactory();
            jobSystem.Initialize(jobIdFactory);

            var jobSystemProvider = new JobSystemProvider(getJobSystem: () => jobSystem);
            var jobSystemProviders = new JobSystemProviders(stage => jobSystemProvider, () => jobSystemProvider);

            TimeSources.InjectProvider(timeSourceProvider);
            JobSystems.InjectProvider(jobSystemProviders);
        }

        [TearDown]
        public void AfterEachTest() {
            var jobSystem = JobSystems.Get<JobSystemAction>();
            jobSystem.DeInitialize();

            JobSystems.InjectProvider(null);
            TimeSources.InjectProvider(null);
        }

        [Test]
        public void Action_IsNotCalled_BeforeStart() {
            bool isInvoked = false;
            Jobs.Action(() => isInvoked = true);
            Assert.IsTrue(!isInvoked);
        }

        [Test]
        public void Action_IsCalled_AtStart() {
            bool isInvoked = false;
            Jobs.Action(() => isInvoked = true).Start();
            Assert.IsTrue(isInvoked);
        }
    }

}
