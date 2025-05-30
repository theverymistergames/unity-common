﻿using MisterGames.Common.Tick;
using NUnit.Framework;
using Utils;

namespace Core {

    public class TimeSourceSubscribersTests {

        [SetUp]
        public void BeforeEachTest() {
            var deltaTimeProvider = new ConstantDeltaTimeProvider(1f);
            var timeSource = new TimeSource(deltaTimeProvider, TimeScaleProviders.Create());
            var timeSourceProvider = new TimeSourceProvider(stage => timeSource);
            
            TimeSources.InjectProvider(timeSourceProvider);
        }

        [TearDown]
        public void AfterEachTest() {
            TimeSources.InjectProvider(null);
        }
        
        [Test]
        public void Subscribers_AreUpdating() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var frameCounter = new CountOnUpdate();

            timeSource.Subscribe(frameCounter);
            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);

            timeSource.Tick();
            Assert.AreEqual(2, frameCounter.Count);
        }

        [Test]
        public void Unsubscribed_AreNotUpdating() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
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
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
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
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var frameCounter = new CountOnUpdate();
            var unsubscribeOnUpdate = new ActionOnUpdate(update => timeSource.Unsubscribe(frameCounter));

            timeSource.Subscribe(unsubscribeOnUpdate);
            timeSource.Subscribe(frameCounter);

            timeSource.Tick();
            Assert.True(frameCounter.Count <= 1);

            timeSource.Tick();
            Assert.True(frameCounter.Count <= 1);
        }

        [Test]
        public void CanUnsubscribeOther_InUpdateLoop_And_WillBeUnsubscribed_AtEndOfTick_DescendingOrder() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var frameCounter = new CountOnUpdate();
            var unsubscribeOnUpdate = new ActionOnUpdate(update => timeSource.Unsubscribe(frameCounter));

            timeSource.Subscribe(frameCounter);
            timeSource.Subscribe(unsubscribeOnUpdate);

            timeSource.Tick();
            Assert.True(frameCounter.Count <= 1);

            timeSource.Tick();
            Assert.True(frameCounter.Count <= 1);
        }
        
        [Test]
        public void CanResubscribeOther_InUpdateLoop() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var frameCounter = new CountOnUpdate();
            var unsubscribeOnUpdate = new ActionOnUpdate(update => {
                timeSource.Unsubscribe(frameCounter);
                timeSource.Subscribe(frameCounter);
            });

            timeSource.Subscribe(frameCounter);
            timeSource.Subscribe(unsubscribeOnUpdate);

            timeSource.Tick();
            timeSource.Tick();
        }
        
        [Test]
        public void CanResubscribe_BeforeUpdates() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);

            var nothingOnUpdate0 = new ActionOnUpdate(update => {});
            var nothingOnUpdate1 = new ActionOnUpdate(update => {});
            var nothingOnUpdate2 = new ActionOnUpdate(update => {});
            var nothingOnUpdate3 = new ActionOnUpdate(update => {});
            var frameCounter = new CountOnUpdate();

            timeSource.Subscribe(nothingOnUpdate0);
            
            timeSource.Subscribe(nothingOnUpdate1);
            timeSource.Unsubscribe(nothingOnUpdate1);
            
            timeSource.Subscribe(nothingOnUpdate2);
            
            timeSource.Subscribe(frameCounter);
            
            timeSource.Subscribe(nothingOnUpdate3);
            timeSource.Unsubscribe(nothingOnUpdate3);

            timeSource.Tick();
            Assert.AreEqual(1, frameCounter.Count);
            
            timeSource.Tick();
            Assert.AreEqual(2, frameCounter.Count);
        }

        [Test]
        public void CanUnsubscribeSelf_InUpdateLoop() {
            var timeSource = (TimeSource) TimeSources.Get(PlayerLoopStage.Update);
            var unsubscribeOnUpdate = new ActionOnUpdate(update => timeSource.Unsubscribe(update));

            timeSource.Subscribe(unsubscribeOnUpdate);
            timeSource.Tick();
            Assert.IsTrue(!timeSource.Unsubscribe(unsubscribeOnUpdate));
        }
    }

}
