﻿using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConditionMaxDirectViewDistance : ICondition, IDependency {

        [Min(0f)] public float maxDistance;

        public bool IsMatched =>
            _user.IsInDirectView(_interactive, out float distance) &&
            distance <= maxDistance * maxDistance;

        private IInteractive _interactive;
        private IInteractiveUser _user;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<Interactive>()
                .Add<InteractiveUser>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _interactive = resolver.Resolve<IInteractive>();
            _user = resolver.Resolve<IInteractiveUser>();
        }
    }

}