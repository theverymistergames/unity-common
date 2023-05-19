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
