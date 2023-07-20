using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConditionIsDetected : ICondition, IDependency {

        public bool shouldBeDetected;

        public bool IsMatched => shouldBeDetected == _detectable.IsDetectedBy(_detector);

        private IDetector _detector;
        private IDetectable _detectable;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<Detector>()
                .Add<Detectable>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _detector = resolver.Resolve<IDetector>();
            _detectable = resolver.Resolve<IDetectable>();
        }
    }

}
