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
