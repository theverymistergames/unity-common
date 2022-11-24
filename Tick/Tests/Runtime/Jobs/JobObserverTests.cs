using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobObserverTests {

        [Test]
        public void NewObserver_IsCompleted() {
            var observer = new JobObserver();
            Assert.IsTrue(observer.IsCompleted);
        }

        [Test]
        public void CanObserveJobs_ThatAddedForObservation_Sequentially() {
            var observer = new JobObserver();

            var job0 = new ActionOnStartJob();
            observer.Observe(job0);
            Assert.IsTrue(!observer.IsCompleted);

            job0.ForceComplete();
            Assert.IsTrue(observer.IsCompleted);

            var job1 = new ActionOnStartJob();
            observer.Observe(job1);
            Assert.IsTrue(!observer.IsCompleted);

            job1.ForceComplete();
            Assert.IsTrue(observer.IsCompleted);
        }

        [Test]
        public void ObserverIsCompleted_IfAddedJobIsCompleted() {
            var observer = new JobObserver();

            var job0 = new ActionOnStartJob();
            job0.ForceComplete();

            observer.Observe(job0);
            Assert.IsTrue(observer.IsCompleted);
        }
    }

}
