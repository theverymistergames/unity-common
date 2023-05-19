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
