using MisterGames.Tick.Core;
using NUnit.Framework;
using Utils;

namespace Core {

    public class TimeSourceSubscribersTests {

        [Test]
        public void Subscribers_AreUpdating() {
            var timeProvider = new ConstantTimeProvider(1f);
            var timeSource = new TimeSource(timeProvider);
            var frameCounter = new CountOnUpdate();

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);

            timeSource.Tick();
            Assert.AreEqual(2, frameCounter.Count);
        }

        [Test]
        public void Unsubscribed_AreNotUpdating() {
            var timeProvider = new ConstantTimeProvider(1f);
            var timeSource = new TimeSource(timeProvider);
            var frameCounter = new CountOnUpdate();

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);

            timeSource.Unsubscribe(frameCounter);
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);
        }

        [Test]
        public void CanSubscribeOther_InUpdateLoop_And_WillBeSubscribed_OnNextTick() {
            var timeProvider = new ConstantTimeProvider(1f);
            var timeSource = new TimeSource(timeProvider);
            var frameCounter = new CountOnUpdate();
            var subscribeOnUpdate = new ActionOnUpdate(update => timeSource.Subscribe(frameCounter));

            timeSource.Subscribe(subscribeOnUpdate);
            timeSource.Tick();
            Assert.AreEqual(0, frameCounter.Count);

            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);
        }

        [Test]
        public void CanUnsubscribeOther_InUpdateLoop_And_WillBeUnsubscribed_AtEndOfTick_AscendingOrder() {
            var timeProvider = new ConstantTimeProvider(1f);
            var timeSource = new TimeSource(timeProvider);
            var frameCounter = new CountOnUpdate();
            var unsubscribeOnUpdate = new ActionOnUpdate(update => timeSource.Unsubscribe(frameCounter));

            timeSource.Subscribe(unsubscribeOnUpdate);
            timeSource.Subscribe(frameCounter);

            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);

            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);
        }

        [Test]
        public void CanUnsubscribeOther_InUpdateLoop_And_WillBeUnsubscribed_AtEndOfTick_DescendingOrder() {
            var timeProvider = new ConstantTimeProvider(1f);
            var timeSource = new TimeSource(timeProvider);
            var frameCounter = new CountOnUpdate();
            var unsubscribeOnUpdate = new ActionOnUpdate(update => timeSource.Unsubscribe(frameCounter));

            timeSource.Subscribe(frameCounter);
            timeSource.Subscribe(unsubscribeOnUpdate);
            timeSource.Tick();
            Assert.AreEqual(0, frameCounter.Count);
        }

        [Test]
        public void CanUnsubscribeSelf_InUpdateLoop() {
            var timeProvider = new ConstantTimeProvider(1f);
            var timeSource = new TimeSource(timeProvider);
            var unsubscribeOnUpdate = new ActionOnUpdate(update => timeSource.Unsubscribe(update));

            timeSource.Subscribe(unsubscribeOnUpdate);
            timeSource.Tick();
            Assert.IsTrue(!timeSource.Unsubscribe(unsubscribeOnUpdate));
        }
    }

}
