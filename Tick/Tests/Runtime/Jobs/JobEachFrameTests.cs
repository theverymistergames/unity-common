﻿using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using NUnit.Framework;
using Utils;

namespace JobTests {

    public class JobEachFrameTests {

        [Test]
        public void EachFrameAction_IsCalledEachTick() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var job = Jobs.EachFrame(() => counter++);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.AreEqual(0, counter);

            timeSource.Tick();
            Assert.AreEqual(1, counter);

            timeSource.Tick();
            Assert.AreEqual(2, counter);
        }

        [Test]
        public void EachFrameAction_CanBeStopped() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var job = Jobs.EachFrame(() => counter++);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.AreEqual(1, counter);

            job.Stop();
            timeSource.Tick();
            Assert.AreEqual(1, counter);

            timeSource.Tick();
            Assert.AreEqual(1, counter);
        }

        [Test]
        public void EachFrameAction_CanBeStarted_AfterStop() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            int counter = 0;
            var job = Jobs.EachFrame(() => counter++);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            timeSource.Tick();
            Assert.AreEqual(1, counter);

            job.Stop();
            timeSource.Tick();
            timeSource.Tick();
            Assert.AreEqual(1, counter);

            job.Start();
            timeSource.Tick();
            Assert.AreEqual(2, counter);

            timeSource.Tick();
            Assert.AreEqual(3, counter);
        }

        [Test]
        public void EachFrameWhile_IsCompleting_AfterActionWhileReturnsFalse() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            bool canContinue = true;

            // ReSharper disable once AccessToModifiedClosure
            var job = Jobs.EachFrameWhile(() => canContinue);

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Run(job);
            Assert.IsTrue(!job.IsCompleted);

            timeSource.Tick();
            Assert.IsTrue(!job.IsCompleted);

            canContinue = false;
            timeSource.Tick();
            Assert.IsTrue(job.IsCompleted);
        }
    }

}
