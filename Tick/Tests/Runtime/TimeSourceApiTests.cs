using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using NUnit.Framework;

namespace Core {

    public class TimeSourceApiTests {

        [Test]
        public void Test_ApiCalls_Sequentially() {
            var timeSource = new TimeSource();

            timeSource.Initialize(new ConstantTimeProvider(1f));
            timeSource.Enable();
            timeSource.Tick();
            timeSource.Disable();
            timeSource.DeInitialize();
        }

        [Test]
        public void Can_InitializeAndEnable() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            timeSource.Initialize(timeProvider);
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));

            timeSource.Enable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(1f));

            timeSource.DeInitialize();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));

            timeSource.Enable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));

            timeSource.Initialize(timeProvider);
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(1f));
        }

        [Test]
        public void Can_EnableDisable_If_Initialized() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            timeSource.Initialize(timeProvider);

            timeSource.Enable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(1f));

            timeSource.Disable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));

            timeSource.Enable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(1f));

            timeSource.DeInitialize();
            timeSource.Initialize(timeProvider);

            timeSource.Enable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(1f));

            timeSource.Disable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));

            timeSource.Enable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(1f));
        }

        [Test]
        public void Cannot_EnableDisable_If_NotInitialized() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);

            timeSource.Enable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));

            timeSource.Disable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));

            timeSource.Enable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));

            timeSource.Initialize(timeProvider);
            timeSource.DeInitialize();

            timeSource.Enable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));

            timeSource.Disable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));

            timeSource.Enable();
            Assert.IsTrue(timeSource.DeltaTime.IsNearlyEqual(0f));
        }

        [Test]
        public void Can_Disable_InUpdateLoop_And_UpdateLoop_WillBeFinished_AscendingOrder() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new CountOnUpdate();
            var disableOnUpdate = new ActionOnUpdate(update => timeSource.Disable());

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(frameCounter);
            timeSource.Subscribe(disableOnUpdate);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);
        }

        [Test]
        public void Can_Disable_InUpdateLoop_And_UpdateLoop_WillBeFinished_DescendingOrder() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new CountOnUpdate();
            var disableOnUpdate = new ActionOnUpdate(update => timeSource.Disable());

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(disableOnUpdate);
            timeSource.Subscribe(frameCounter);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);
        }

        [Test]
        public void Can_DeInitialize_InUpdateLoop_And_UpdateLoop_WillBeFinished_AscendingOrder() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new CountOnUpdate();
            var disableOnUpdate = new ActionOnUpdate(update => timeSource.DeInitialize());

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(frameCounter);
            timeSource.Subscribe(disableOnUpdate);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);
        }

        [Test]
        public void Can_DeInitialize_InUpdateLoop_And_UpdateLoop_WillBeFinished_DescendingOrder() {
            var timeSource = new TimeSource();
            var timeProvider = new ConstantTimeProvider(1f);
            var frameCounter = new CountOnUpdate();
            var disableOnUpdate = new ActionOnUpdate(update => timeSource.DeInitialize());

            timeSource.Initialize(timeProvider);
            timeSource.Enable();

            timeSource.Subscribe(disableOnUpdate);
            timeSource.Subscribe(frameCounter);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);

            timeSource.Tick();
            Assert.IsTrue(frameCounter.Count == 1);
        }
    }

}
