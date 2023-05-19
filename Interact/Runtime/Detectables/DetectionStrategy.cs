using MisterGames.Common.Attributes;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [CreateAssetMenu(fileName = nameof(DetectionStrategy), menuName = "MisterGames/Interactives/" + nameof(DetectionStrategy))]
    public sealed class DetectionStrategy : ScriptableObject, IDependency {

        [SerializeReference] [SubclassSelector] private ICondition _startConstraint;
        [SerializeReference] [SubclassSelector] private ICondition _continueConstraint;

        public void OnAddDependencies(IDependencyContainer container) {
            if (_startConstraint is IDependency s) s.OnAddDependencies(container);
            if (_continueConstraint is IDependency c) c.OnAddDependencies(container);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            if (_startConstraint is IDependency s) s.OnResolveDependencies(resolver);
            if (_continueConstraint is IDependency c) c.OnResolveDependencies(resolver);
        }

        public bool IsAllowedToStartDetection() {
            return _startConstraint == null || _startConstraint.IsMatched;
        }

        public bool IsAllowedToContinueDetection() {
            return _continueConstraint == null || _continueConstraint.IsMatched;
        }
    }

}
