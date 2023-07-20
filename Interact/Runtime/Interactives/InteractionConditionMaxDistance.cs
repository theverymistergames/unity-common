using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConditionMaxDistance : ICondition, IDependency {

        [Min(0f)] public float maxDistance;

        public bool IsMatched =>
            Vector3.SqrMagnitude(_user.Transform.position - _interactive.Transform.position) <=
            maxDistance * maxDistance;

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
