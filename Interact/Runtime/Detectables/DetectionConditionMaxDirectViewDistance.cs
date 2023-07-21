﻿using System;
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