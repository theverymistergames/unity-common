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

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<IDetector>(this);
            container.AddDependency<IDetectable>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _detector = resolver.ResolveDependency<IDetector>();
            _detectable = resolver.ResolveDependency<IDetectable>();
        }
    }

}
