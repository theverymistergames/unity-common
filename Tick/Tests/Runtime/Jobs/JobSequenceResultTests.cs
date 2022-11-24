using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobSequenceResultTests {

        [Test]
        public void NextJob_InSequence_CanReceiveResult_FromPreviousResultJob_Test() {
            NextJob_InSequence_CanReceiveResult_FromPreviousResultJob(1);
            NextJob_InSequence_CanReceiveResult_FromPreviousResultJob(2f);
            NextJob_InSequence_CanReceiveResult_FromPreviousResultJob("abc");
            NextJob_InSequence_CanReceiveResult_FromPreviousResultJob<object>(null);
            NextJob_InSequence_CanReceiveResult_FromPreviousResultJob(new object());
            NextJob_InSequence_CanReceiveResult_FromPreviousResultJob(new ActionOnStartJob());
        }

        private static void NextJob_InSequence_CanReceiveResult_FromPreviousResultJob<R>(R input) {
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

            Assert.AreEqual(input, resultJob1.Result);
        }

        [Test]
        public void LastResultJob_InSequence_IsSequenceResultJob() {
            LastResultJob_InSequence_IsSequenceResultJob_Test(1);
            LastResultJob_InSequence_IsSequenceResultJob_Test(2f);
            LastResultJob_InSequence_IsSequenceResultJob_Test("abc");
            LastResultJob_InSequence_IsSequenceResultJob_Test<object>(null);
            LastResultJob_InSequence_IsSequenceResultJob_Test(new object());
            LastResultJob_InSequence_IsSequenceResultJob_Test(new ActionOnStartJob());
        }

        private static void LastResultJob_InSequence_IsSequenceResultJob_Test<R>(R input) {
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

            if (input == null) {
                Assert.IsTrue(resultJob.Result == null);
                Assert.IsTrue(sequence.Result == null);
                return;
            }

            Assert.AreEqual(input, resultJob.Result);
            Assert.AreEqual(input, sequence.Result);
        }
    }

}
