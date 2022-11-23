using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace Jobs {

    public class JobSequenceResultTests {

        [Test]
        [TestCase(1)]
        [TestCase(2f)]
        [TestCase("abc")]
        public void NextJob_InSequence_CanReceiveResult_FromPreviousResultJob<R>(R input) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var sequence = JobSequence.Create()
                .Add(new ResultJob<R>(), out var resultJob0)
                .Add(new ResultJob<R>(j => j.ForceComplete(resultJob0.Result)), out var resultJob1);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(sequence);
            Assert.IsTrue(!sequence.IsCompleted);
            Assert.IsTrue(!resultJob0.IsCompleted);
            Assert.IsTrue(!resultJob1.IsCompleted);

            ((ResultJob<R>) resultJob0).ForceComplete(input);
            timeSource.Tick();
            Assert.IsTrue(sequence.IsCompleted);
            Assert.IsTrue(resultJob0.IsCompleted);
            Assert.IsTrue(resultJob1.IsCompleted);
            Assert.IsTrue(resultJob1.Result.Equals(input));
        }

        [Test]
        [TestCase(1)]
        [TestCase(2f)]
        [TestCase("abc")]
        public void LastResultJob_InSequence_IsSequenceResultJob<R>(R input) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var sequence = JobSequence.Create()
                .Add(new ResultJob<R>(j => j.ForceComplete(input)), out var resultJob)
                .Add(new ActionOnStartJob(j => j.ForceComplete()));

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(sequence);
            Assert.IsTrue(sequence.IsCompleted);
            Assert.IsTrue(resultJob.IsCompleted);
            Assert.IsTrue(resultJob.Result.Equals(input));
            Assert.IsTrue(sequence.Result.Equals(input));
        }
    }

}
