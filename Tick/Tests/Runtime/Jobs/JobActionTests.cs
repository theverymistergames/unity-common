using System;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobActionTests {

        [Test]
        public void NotCompletedAction_HasProgressValueZero() {
            var job = Jobs.Action(() => { });
            Assert.AreEqual(0f, job.Progress);
        }

        [Test]
        public void CompletedAction_HasProgressValueOne() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var job = Jobs.Action(() => { });

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.AreEqual(1f, job.Progress);
        }

        [Test]
        public void Action_IsCompleting_AtStart() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var action = new Action(() => counter++);
            var job = Jobs.Action(action);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.AreEqual(1, counter);
            Assert.IsTrue(job.IsCompleted);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2f)]
        [TestCase("abc")]
        public void ActionResult_IsCompleting_AtStart_AndHasResult<R>(R input) {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            var action = new Func<R>(() => input);
            var job = Jobs.Action(action);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.AreEqual(input, job.Result);
            Assert.IsTrue(job.IsCompleted);
        }
    }

}
