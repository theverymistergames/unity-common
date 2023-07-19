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

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<IInteractive>()
                .Add<IInteractiveUser>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            resolver
                .Resolve(out _interactive)
                .Resolve(out _user);
        }
    }

}
