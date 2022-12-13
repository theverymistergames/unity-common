using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobSequenceTests {

        [SetUp]
        public void BeforeEachTest() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var timeSourceProvider = new TimeSourceProvider(stage => timeSource);

            var jobIdFactory = new NextJobIdFactory();
            var jobSystemAction = new JobSystemAction();
            var jobSystemDelay = new JobSystemDelay();
            var jobSystemSequence = new JobSystemSequence();

            jobSystemAction.Initialize(jobIdFactory);
            jobSystemDelay.Initialize(jobIdFactory);
            jobSystemSequence.Initialize(jobIdFactory);

            timeSource.Subscribe(jobSystemDelay);
            timeSource.Subscribe(jobSystemSequence);

            var jobSystemProvider = new JobSystemProvider(getJobSystem: type => {
                if (type == typeof(JobSystemDelay)) return jobSystemDelay;
                if (type == typeof(JobSystemAction)) return jobSystemAction;
                if (type == typeof(JobSystemSequence)) return jobSystemSequence;
                return null;
            });

            var jobSystemProviders = new JobSystemProviders(stage => jobSystemProvider, () => jobSystemProvider);

            TimeSources.InjectProvider(timeSourceProvider);
            JobSystems.InjectProvider(jobSystemProviders);
        }

        [TearDown]
        public void AfterEachTest() {
            var timeSource = TimeSources.Get(PlayerLoopStage.Update);
            var jobSystemAction = JobSystems.Get<JobSystemAction>();
            var jobSystemDelay = JobSystems.Get<JobSystemDelay>();
            var jobSystemSequence = JobSystems.Get<JobSystemSequence>();

            jobSystemAction.DeInitialize();
            jobSystemDelay.DeInitialize();
            jobSystemSequence.DeInitialize();

            timeSource.Unsubscribe(jobSystemDelay);
            timeSource.Unsubscribe(jobSystemSequence);

            JobSystems.InjectProvider(null);
            TimeSources.InjectProvider(null);
        }

        [Test]
        public void EmptySequence_IsCompleted() {
            var sequence = JobSequence.Create().Start();
            Assert.IsTrue(sequence.IsCompleted);
        }

        [Test]
        public void IsNotStarting_SequenceJobs_IfNotStarted() {
            var job0 = Jobs.Action(() => { });
            var job1 = Jobs.Action(() => { });
            var job2 = Jobs.Action(() => { });

            JobSequence.Create()
                .Add(job0)
                .Add(job1)
                .Add(job2);

            Assert.IsTrue(!job0.IsCompleted);
            Assert.IsTrue(!job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);
        }

        [Test]
        public void IsCompleting_Sequence_IfJobsCompleting_AtStart() {
            var job0 = Jobs.Action(() => { });
            var job1 = Jobs.Action(() => { });
            var job2 = Jobs.Action(() => { });

            var sequence = JobSequence.Create()
                .Add(job0)
                .Add(job1)
                .Add(job2)
                .Start();

            Assert.IsTrue(job0.IsCompleted);
            Assert.IsTrue(job1.IsCompleted);
            Assert.IsTrue(job2.IsCompleted);
            Assert.IsTrue(sequence.IsCompleted);
        }

        [Test]
        public void IsCompleting_Jobs_Till_UnableToComplete_AtStart() {
            var job0 = Jobs.Action(() => { });
            var job1 = Jobs.Action(() => { });
            var job2 = Jobs.Delay(1f);
            var job3 = Jobs.Delay(1f);

            var sequence = JobSequence.Create()
                .Add(job0)
                .Add(job1)
                .Add(job2)
                .Add(job3)
                .Start();

            Assert.IsTrue(job0.IsCompleted);
            Assert.IsTrue(job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);
            Assert.IsTrue(!job3.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);
        }

        [Test]
        public void IsCompleting_Jobs_Till_UnableToComplete_InUpdateLoop() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            var job0 = Jobs.Action(() => { });
            var job1 = Jobs.Action(() => { });
            var job2 = Jobs.Delay(1f);
            var job3 = Jobs.Action(() => { });
            var job4 = Jobs.Action(() => { });

            var sequence = JobSequence.Create()
                .Add(job0)
                .Add(job1)
                .Add(job2)
                .Add(job3)
                .Add(job4)
                .Start();

            Assert.IsTrue(job0.IsCompleted);
            Assert.IsTrue(job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);
            Assert.IsTrue(!job3.IsCompleted);
            Assert.IsTrue(!job4.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job2.IsCompleted);
            Assert.IsTrue(job3.IsCompleted);
            Assert.IsTrue(job4.IsCompleted);
            Assert.IsTrue(sequence.IsCompleted);
        }

        [Test]
        public void IsCompleting_Sequence_IfJobsCompleting_InUpdateLoop() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            var job0 = Jobs.Delay(1f);
            var job1 = Jobs.Delay(1f);
            var job2 = Jobs.Delay(1f);

            var sequence = JobSequence.Create()
                .Add(job0)
                .Add(job1)
                .Add(job2)
                .Start();

            Assert.IsTrue(!job0.IsCompleted);
            Assert.IsTrue(!job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job0.IsCompleted);
            Assert.IsTrue(!job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job2.IsCompleted);
            Assert.IsTrue(sequence.IsCompleted);
        }

        [Test]
        public void IsNotUpdating_Sequence_IfStopped() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var job = Jobs.Delay(2f);

            var sequence = JobSequence.Create()
                .Add(job)
                .Start();

            Assert.IsTrue(!job.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);

            sequence.Stop();
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);
        }

        [Test]
        public void CanStop_ThenRestart_Sequence() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var job = Jobs.Delay(2f);

            var sequence = JobSequence.Create()
                .Add(job)
                .Start();

            Assert.IsTrue(!job.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);

            sequence.Stop();
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);

            sequence.Start();
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
            Assert.IsTrue(sequence.IsCompleted);
        }

        [Test]
        public void SeveralSequences_CanBeCreated_InParallel() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            var sequenceBuilder0 = JobSequence.Create();
            var sequenceBuilder1 = JobSequence.Create();

            var job0 = Jobs.Delay(1f);
            sequenceBuilder0.Add(job0);

            var job1 = Jobs.Delay(1f);
            sequenceBuilder1.Add(job1);

            var job2 = Jobs.Delay(1f);
            sequenceBuilder0.Add(job2);

            var job3 = Jobs.Delay(1f);
            sequenceBuilder1.Add(job3);

            var sequence0 = sequenceBuilder0.Start();
            var sequence1 = sequenceBuilder1.Start();

            Assert.IsTrue(!job0.IsCompleted);
            Assert.IsTrue(!job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);
            Assert.IsTrue(!job3.IsCompleted);
            Assert.IsTrue(!sequence0.IsCompleted);
            Assert.IsTrue(!sequence1.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job0.IsCompleted);
            Assert.IsTrue(job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);
            Assert.IsTrue(!job3.IsCompleted);
            Assert.IsTrue(!sequence0.IsCompleted);
            Assert.IsTrue(!sequence1.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job2.IsCompleted);
            Assert.IsTrue(job3.IsCompleted);
            Assert.IsTrue(sequence0.IsCompleted);
            Assert.IsTrue(sequence1.IsCompleted);
        }
    }

}
