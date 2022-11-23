using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobRunningTests {

        [Test]
        public void Starts_Job_Immediately_AtRun() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var job = new ActionOnStartJob();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(job.IsStarted);
        }

        [Test]
        public void IsNotSubscribing_UpdateJob_If_CompletedBeforeStart() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var job = new ActionOnUpdateJob();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            job.ForceComplete();
            timeSource.Run(job);

            Assert.IsTrue(!timeSource.Unsubscribe(job));
        }

        [Test]
        public void IsUpdating_UpdateJob_IfNot_CompletedAtStart() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var job = new CountOnUpdateJob();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.IsTrue(job.Count == 1);

            timeSource.Tick();
            Assert.IsTrue(job.Count == 2);
        }

        [Test]
        public void IsUnsubscribing_UpdateJob_IfCompleted_BeforeUpdateLoop() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var job = new ActionOnUpdateJob();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            job.ForceComplete();
            timeSource.Tick();
            Assert.IsTrue(!timeSource.Unsubscribe(job));
        }

        [Test]
        public void IsUnsubscribing_UpdateJob_IfCompleted_InUpdateLoop() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var job = new ActionOnUpdateJob(j => j.ForceComplete());

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.IsTrue(!timeSource.Unsubscribe(job));
        }
    }

}
