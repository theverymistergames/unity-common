using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConditionInDirectView : ICondition, IDependency {

        public bool shouldBeInDirectView;

        public bool IsMatched => shouldBeInDirectView == _detector.IsInDirectView(_detectable, out _);

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
