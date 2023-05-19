using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConditionIsInteracting : ICondition, IDependency {

        public bool shouldBeInInteraction;

        public bool IsMatched => shouldBeInInteraction == _user.IsInteractingWith(_interactive);

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
