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

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<IInteractive>(this);
            container.AddDependency<IInteractiveUser>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _interactive = resolver.ResolveDependency<IInteractive>();
            _user = resolver.ResolveDependency<IInteractiveUser>();
        }
    }

}
