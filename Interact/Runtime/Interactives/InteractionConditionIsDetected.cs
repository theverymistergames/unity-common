using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConditionIsDetected : ICondition, IDependency {

        public bool shouldBeDetected;

        public bool IsMatched => shouldBeDetected == _user.IsDetected(_interactive);

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
