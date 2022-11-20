using MisterGames.Common.Lists;
using MisterGames.Tick.Core;
using MisterGames.Tick.Utils;
using NUnit.Framework;

namespace Core {

    public class JobLaunchTests {

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
        public void IsSubscribing_UpdateJob_IfNot_CompletedAtStart() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var job = new ActionOnUpdateJob();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);

            Assert.IsTrue(timeSource.Subscribers.Contains(job));
        }

        [Test]
        public void IsNotSubscribing_UpdateJob_If_CompletedAtStart() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var job = new ActionOnUpdateJob();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            job.ForceComplete();
            timeSource.Run(job);

            Assert.IsTrue(!timeSource.Subscribers.Contains(job));
        }


    }

}
