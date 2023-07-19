using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractionConditionInDirectView : ICondition, IDependency {

        public bool shouldBeInDirectView;

        public bool IsMatched => shouldBeInDirectView == _user.IsInDirectView(_interactive, out _);

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
