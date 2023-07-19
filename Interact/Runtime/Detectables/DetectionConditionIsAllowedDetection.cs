using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConditionIsAllowedDetection : ICondition, IDependency {

        public Optional<bool> shouldBeAllowedToStartDetection;
        public Optional<bool> shouldBeAllowedToContinueDetection;

        public bool IsMatched =>
            shouldBeAllowedToStartDetection.IsEmptyOrEquals(_detectable.IsAllowedToStartDetectBy(_detector)) &&
            shouldBeAllowedToContinueDetection.IsEmptyOrEquals(_detectable.IsAllowedToContinueDetectBy(_detector));

        private IDetector _detector;
        private IDetectable _detectable;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<IDetector>()
                .Add<IDetectable>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            resolver
                .Resolve(out _detector)
                .Resolve(out _detectable);
        }
    }

}
