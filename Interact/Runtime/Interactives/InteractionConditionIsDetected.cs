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
