using MisterGames.Common.Attributes;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [CreateAssetMenu(fileName = nameof(InteractionStrategy), menuName = "MisterGames/Interactives/" + nameof(InteractionStrategy))]
    public sealed class InteractionStrategy : ScriptableObject, IDependency {

        [SerializeField] private bool _allowStopImmediatelyAfterStart;
        [SerializeReference] [SubclassSelector] private ICondition _readyConstraint;
        [SerializeReference] [SubclassSelector] private ICondition _startConstraint;
        [SerializeReference] [SubclassSelector] private ICondition _continueConstraint;

        private IInteractive _interactive;
        private IInteractiveUser _userDependency;

        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<IInteractive>(this);
            container.AddDependency<IInteractiveUser>(this);

            if (_readyConstraint is IDependency r) r.OnAddDependencies(container);
            if (_startConstraint is IDependency s) s.OnAddDependencies(container);
            if (_continueConstraint is IDependency c) c.OnAddDependencies(container);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _interactive = resolver.ResolveDependency<IInteractive>();
            _userDependency = resolver.ResolveDependency<IInteractiveUser>();

            if (_readyConstraint is IDependency r) r.OnResolveDependencies(resolver);
            if (_startConstraint is IDependency s) s.OnResolveDependencies(resolver);
            if (_continueConstraint is IDependency c) c.OnResolveDependencies(resolver);
        }

        public bool IsReadyToStartInteraction() {
            return _readyConstraint is { IsMatched: true };
        }

        public bool IsAllowedToStartInteraction() {
            return _startConstraint is { IsMatched: true };
        }

        public bool IsAllowedToContinueInteraction() {
            if (_allowStopImmediatelyAfterStart) {
                return _continueConstraint is { IsMatched: true };
            }

            if (_interactive.TryGetInteractionStartTime(_userDependency, out int startTime) &&
                startTime >= TimeSources.frameCount
            ) {
                return true;
            }

            return _continueConstraint is { IsMatched: true };
        }
    }

}
