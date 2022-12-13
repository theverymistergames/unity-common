using MisterGames.Tick.Jobs;
using NUnit.Framework;

namespace JobTests {

    public class JobCompetedTests {

        [Test]
        public void CompletedJob_IsCompleted() {
            var job = Jobs.Completed;
            Assert.IsTrue(job.IsCompleted);
        }
    }

}
