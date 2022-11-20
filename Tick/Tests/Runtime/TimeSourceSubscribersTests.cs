﻿using MisterGames.Common.Lists;
using MisterGames.Tick.Core;
using NUnit.Framework;

namespace Core {

    public class TimeSourceSubscribersTests {

        [Test]
        public void Subscribers_AreUpdating() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new FrameCounterUpdate();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 2);
        }

        [Test]
        public void Unsubscribed_AreNotUpdating() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new FrameCounterUpdate();

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.Unsubscribe(frameCounter);
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);
        }

        [Test]
        public void CanSubscribeOther_InUpdateLoop_And_WillBeSubscribed_OnNextTick() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new FrameCounterUpdate();
            var subscribeOnUpdate = new ActionOnUpdate(update => timeSource.Subscribe(frameCounter));

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(subscribeOnUpdate);
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 0);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);
        }

        [Test]
        public void CanUnsubscribeOther_InUpdateLoop_AscendingOrder() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new FrameCounterUpdate();
            var unsubscribeOnUpdate = new ActionOnUpdate(update => timeSource.Unsubscribe(frameCounter));

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(unsubscribeOnUpdate);
            timeSource.Tick();

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);
        }

        [Test]
        public void CanUnsubscribeOther_InUpdateLoop_DescendingOrder() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new FrameCounterUpdate();
            var unsubscribeOnUpdate = new ActionOnUpdate(update => timeSource.Unsubscribe(frameCounter));

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.Subscribe(unsubscribeOnUpdate);
            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);
        }

        [Test]
        public void CanUnsubscribeSelf_InUpdateLoop() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var unsubscribeOnUpdate = new ActionOnUpdate(update => timeSource.Unsubscribe(update));

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(unsubscribeOnUpdate);
            timeSource.Tick();
            Assert.IsTrue(!timeSource.Subscribers.Contains(unsubscribeOnUpdate));
        }
    }

}
