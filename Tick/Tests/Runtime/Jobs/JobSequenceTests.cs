using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobSequenceTests {

        [Test]
        public void IsCompleting_EmptySequence_AtStart() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(sequence);
            Assert.IsTrue(sequence.IsCompleted);
        }

        [Test]
        public void IsCompleting_Sequence_IfJobsCompleting_AtStart() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job0 = new ActionOnStartJob(j => j.ForceComplete());
            var job1 = new ActionOnStartJob(j => j.ForceComplete());
            var job2 = new ActionOnStartJob(j => j.ForceComplete());

            sequence.Add(job0).Add(job1).Add(job2);

            timeSource.Run(sequence);

            Assert.IsTrue(sequence.IsCompleted);

            Assert.IsTrue(job0.IsCompleted);
            Assert.IsTrue(job1.IsCompleted);
            Assert.IsTrue(job2.IsCompleted);
        }

        [Test]
        public void IsCompleting_Jobs_Till_UnableToComplete_AtStart() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job0 = new ActionOnStartJob(j => j.ForceComplete());
            var job1 = new ActionOnStartJob(j => j.ForceComplete());
            var job2 = new ActionOnStartJob();
            var job3 = new ActionOnStartJob();

            sequence.Add(job0).Add(job1).Add(job2).Add(job3);

            timeSource.Run(sequence);

            Assert.IsTrue(!sequence.IsCompleted);

            Assert.IsTrue(job0.IsCompleted);
            Assert.IsTrue(job1.IsCompleted);
            Assert.IsTrue(!job2.IsCompleted);
            Assert.IsTrue(!job3.IsCompleted);
        }

        [Test]
        public void IsCompleting_Sequence_IfJobsCompleting_InUpdateLoop() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job0 = new ActionOnUpdateJob(j => j.ForceComplete());
            var job1 = new ActionOnUpdateJob(j => j.ForceComplete());
            var job2 = new ActionOnUpdateJob(j => j.ForceComplete());

            sequence.Add(job0).Add(job1).Add(job2);
            timeSource.Run(sequence);

            timeSource.Tick();
            Assert.IsTrue(job0.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job1.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job2.IsCompleted);
            Assert.IsTrue(sequence.IsCompleted);
        }

        [Test]
        public void IsCompleting_Jobs_Till_UnableToComplete_InUpdateLoop() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job0 = new ActionOnUpdateJob(j => j.ForceComplete());
            var job1 = new ActionOnUpdateJob(j => j.ForceComplete());
            var job2 = new ActionOnUpdateJob();
            var job3 = new ActionOnUpdateJob();

            sequence.Add(job0).Add(job1).Add(job2).Add(job3);

            timeSource.Run(sequence);
            Assert.IsTrue(!sequence.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job0.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job1.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(job2.IsStarted && !job2.IsCompleted);
            Assert.IsTrue(!sequence.IsCompleted);
        }

        [Test]
        public void IsCompleting_Jobs_AtJobStart_InUpdateLoop() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job0 = new ActionOnUpdateJob(j => j.ForceComplete());
            var job1 = new ActionOnStartJob(j => j.ForceComplete());
            var job2 = new ActionOnStartJob();
            var job3 = new ActionOnUpdateJob();

            sequence.Add(job0).Add(job1).Add(job2).Add(job3);

            timeSource.Run(sequence);
            timeSource.Tick();

            Assert.IsTrue(job0.IsCompleted);
            Assert.IsTrue(job1.IsCompleted);
            Assert.IsTrue(job2.IsStarted && !job2.IsCompleted);
            Assert.IsTrue(!job3.IsStarted);
        }

        [Test]
        public void IsNotUpdating_Sequence_IfStopped_BeforeTick() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job = new CountOnUpdateJob();
            sequence.Add(job);

            timeSource.Run(sequence);
            timeSource.Tick();
            Assert.AreEqual(1, job.Count);

            sequence.Stop();
            timeSource.Tick();
            Assert.AreEqual(1, job.Count);
        }

        [Test]
        public void IsNotUpdating_Sequence_IfStopped_InUpdateLoop_StartingAtNextTick_AscendingOrder() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();
            var stopOnUpdate = new ActionOnUpdate(update => sequence.Stop());

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job = new CountOnUpdateJob();
            sequence.Add(job);

            timeSource.Run(sequence);
            timeSource.Tick();
            Assert.IsTrue(job.Count == 1);

            timeSource.Subscribe(stopOnUpdate);
            timeSource.Tick();
            Assert.AreEqual(1, job.Count);
            Assert.IsTrue(job.IsStopped);
        }

        [Test]
        public void IsNotUpdating_Sequence_IfStopped_InUpdateLoop_StartingAtNextTick_DescendingOrder() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();
            var stopOnUpdate = new ActionOnUpdate(update => sequence.Stop());

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job = new CountOnUpdateJob();
            sequence.Add(job);

            timeSource.Subscribe(stopOnUpdate);
            timeSource.Run(sequence);
            timeSource.Tick();

            Assert.AreEqual(1, job.Count);
            Assert.IsTrue(job.IsStopped);
        }

        [Test]
        public void CanStop_ThenRestart_Sequence_BeforeTick() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job = new CountOnUpdateJob();
            sequence.Add(job);

            timeSource.Run(sequence);
            timeSource.Tick();
            Assert.AreEqual(1, job.Count);

            sequence.Stop();
            timeSource.Tick();
            Assert.AreEqual(1, job.Count);
            Assert.IsTrue(job.IsStopped);

            sequence.Start();
            timeSource.Tick();
            Assert.AreEqual(2, job.Count);
        }

        [Test]
        public void CanStop_ThenRestart_Sequence_InUpdateLoop_AscendingOrder() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();
            var startOnUpdate = new ActionOnUpdate(update => sequence.Start());

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job = new CountOnUpdateJob();
            sequence.Add(job);

            timeSource.Run(sequence);
            sequence.Stop();

            timeSource.Tick();
            Assert.AreEqual(0, job.Count);

            timeSource.Subscribe(startOnUpdate);
            timeSource.Tick();
            Assert.AreEqual(1, job.Count);
        }

        [Test]
        public void CanStop_ThenRestart_Sequence_InUpdateLoop_DescendingOrder() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var sequence = JobSequence.Create();
            var startOnUpdate = new ActionOnUpdate(update => sequence.Start());

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            var job = new CountOnUpdateJob();
            sequence.Add(job);

            timeSource.Subscribe(startOnUpdate);

            timeSource.Run(sequence);
            sequence.Stop();

            timeSource.Tick();
            Assert.AreEqual(0, job.Count);

            timeSource.Tick();
            Assert.AreEqual(1, job.Count);
        }
    }

}
