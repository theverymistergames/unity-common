using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobWaitTests {

        [Test]
        public void WaitAll_IsCompletedBeforeStart_IfEmptyJobArrayPassed() {
            var job = Jobs.WaitAll();
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void WaitAll_IsCompletedBeforeStart_IfCompletedJobsPassed() {
            var job = Jobs.WaitAll(Jobs.Completed);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void Wait_IsCompletedBeforeStart_IfCompletedJobPassed() {
            var job = Jobs.Wait(Jobs.Completed);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void WaitResult_IsCompletedBeforeStart_IfCompletedJobPassed() {
            WaitResult_IsCompletedBeforeStart_IfCompletedJobPassed_Test(2f);
            WaitResult_IsCompletedBeforeStart_IfCompletedJobPassed_Test("abc");
            WaitResult_IsCompletedBeforeStart_IfCompletedJobPassed_Test(1);
            WaitResult_IsCompletedBeforeStart_IfCompletedJobPassed_Test<object>(null);
            WaitResult_IsCompletedBeforeStart_IfCompletedJobPassed_Test(new object());
            WaitResult_IsCompletedBeforeStart_IfCompletedJobPassed_Test(new ActionOnStartJob());
        }

        private static void WaitResult_IsCompletedBeforeStart_IfCompletedJobPassed_Test<R>(R input) {
            var waitForResult = new ResultJob<R>();
            waitForResult.ForceComplete(input);

            var job = Jobs.Wait(waitForResult);
            Assert.IsTrue(job.IsCompleted);
            Assert.AreEqual(job.Result, waitForResult.Result);
        }

        [Test]
        public void Wait_IsNotCompleted_AtStart_IfPassedJobIsNotCompleted() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var waitFor = new ActionOnStartJob();
            var job = Jobs.Wait(waitFor);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void WaitResult_IsNotCompleted_AtStart_IfPassedJobIsNotCompleted() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var waitForResult = new ResultJob<float>();
            var job = Jobs.Wait(waitForResult);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void WaitAll_IsNotCompleted_AtStart_IfPassedJobIsNotCompleted() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var waitFor = new ActionOnStartJob();
            var job = Jobs.WaitAll(waitFor);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);
        }

        [Test]
        public void Wait_IsCompleting_InUpdateLoop_IfPassedJobIsCompleted() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var waitFor = new ActionOnStartJob();
            var job = Jobs.Wait(waitFor);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);

            waitFor.ForceComplete();
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void WaitAll_IsCompleting_InUpdateLoop_IfPassedJobsAreCompleted() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var waitFor0 = new ActionOnStartJob();
            var waitFor1 = new ActionOnStartJob();
            var job = Jobs.WaitAll(waitFor0, waitFor1);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);

            waitFor0.ForceComplete();
            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            waitFor1.ForceComplete();
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        public void WaitResultJob_IsCompleting_InUpdateLoop_IfPassedJobIsCompleted() {
            WaitResultJob_IsCompleting_InUpdateLoop_IfPassedJobIsCompleted_Test(2f);
            WaitResultJob_IsCompleting_InUpdateLoop_IfPassedJobIsCompleted_Test("abc");
            WaitResultJob_IsCompleting_InUpdateLoop_IfPassedJobIsCompleted_Test(1);
            WaitResultJob_IsCompleting_InUpdateLoop_IfPassedJobIsCompleted_Test<object>(null);
            WaitResultJob_IsCompleting_InUpdateLoop_IfPassedJobIsCompleted_Test(new object());
            WaitResultJob_IsCompleting_InUpdateLoop_IfPassedJobIsCompleted_Test(new ActionOnStartJob());
        }

        private static void WaitResultJob_IsCompleting_InUpdateLoop_IfPassedJobIsCompleted_Test<R>(R input) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var waitForResult = new ResultJob<R>();
            var job = Jobs.Wait(waitForResult);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);

            waitForResult.ForceComplete(input);
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
            Assert.AreEqual(input, job.Result);
        }
    }

}
