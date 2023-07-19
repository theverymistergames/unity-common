using System;
using System.Text;
using MisterGames.Common.Attributes;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Common.Conditions {

    [Serializable]
    public sealed class ConditionGroupAll : ITransition, ITransitionCallback, IDependency {

        [SerializeReference] [SubclassSelector] public ICondition[] conditions;

        public bool IsMatched => CheckMatch();

        private ITransitionCallback _externalCallback;

        public void OnSetupDependencies(IDependencyContainer container) {
            for (int i = 0; i < conditions.Length; i++) {
                if (conditions[i] is IDependency dep) dep.OnSetupDependencies(container);
            }
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            for (int i = 0; i < conditions.Length; i++) {
                if (conditions[i] is IDependency dep) dep.OnResolveDependencies(resolver);
            }
        }

        public void Arm(ITransitionCallback callback) {
            _externalCallback = callback;

            for (int i = 0; i < conditions.Length; i++) {
                if (conditions[i] is ITransition transition) transition.Arm(this);
            }
        }

        public void Disarm() {
            for (int i = 0; i < conditions.Length; i++) {
                if (conditions[i] is ITransition transition) transition.Disarm();
            }

            _externalCallback = null;
        }

        public void OnTransitionMatch(ITransition match) {
            if (IsMatched) _externalCallback?.OnTransitionMatch(this);
        }

        private bool CheckMatch() {
            for (int i = 0; i < conditions.Length; i++) {
                if (!conditions[i].IsMatched) return false;
            }

            return true;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(ConditionGroupAll)} {GetHashCode()}, conditions: --->>");

            for (int i = 0; i < conditions.Length; i++) {
                sb.AppendLine($"- {conditions[i]}");
            }

            sb.AppendLine($"{nameof(ConditionGroupAll)} {GetHashCode()} <<---");
            return sb.ToString();
        }
    }

}
