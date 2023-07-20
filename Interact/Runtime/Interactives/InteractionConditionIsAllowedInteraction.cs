using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConditionIsAllowedInteraction : ICondition, IDependency {

        public Optional<bool> shouldBeReadyToStartInteract;
        public Optional<bool> shouldBeAllowedToStartInteract;
        public Optional<bool> shouldBeAllowedToContinueInteract;

        public bool IsMatched =>
            shouldBeReadyToStartInteract.IsEmptyOrEquals(_interactive.IsReadyToStartInteractWith(_user)) &&
            shouldBeAllowedToStartInteract.IsEmptyOrEquals(_interactive.IsAllowedToStartInteractWith(_user)) &&
            shouldBeAllowedToContinueInteract.IsEmptyOrEquals(_interactive.IsAllowedToContinueInteractWith(_user));

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
