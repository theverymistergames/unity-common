using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConditionMaxRadius : ICondition, IDependency {

        [Min(0f)] public float maxRadius;

        public bool IsMatched =>
            Vector3.SqrMagnitude(_detector.Transform.position - _detectable.Transform.position) <= maxRadius * maxRadius;

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
