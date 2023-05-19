using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConditionMaxDirectViewDistance : ICondition, IDependency {

        [Min(0f)] public float maxDistance;

        public bool IsMatched => _detector.IsInDirectView(_detectable, out float distance) &&
                                 distance <= maxDistance * maxDistance;

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
