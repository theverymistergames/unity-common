using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConditionMaxUsers : ICondition, IDependency {

        [Min(0)] public int maxUsers;

        public bool IsMatched => _interactive.Users.Count <= maxUsers;

        private IInteractive _interactive;

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<IInteractive>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _interactive = resolver.ResolveDependency<IInteractive>();
        }
    }

}
