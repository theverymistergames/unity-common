using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Conditions {

    [Serializable]
    public sealed class ConditionGroupAll : ICondition, IConditionCallback, IDynamicDataHost {

        [SerializeReference] [SubclassSelector] public ICondition[] conditions;

        public bool IsMatched => CheckCondition();

        private IConditionCallback _externalCallback;

        public void OnSetDataTypes(HashSet<Type> types) {
            for (int i = 0; i < conditions.Length; i++) {
                if (conditions[i] is IDynamicDataHost host) host.OnSetDataTypes(types);
            }
        }

        public void OnSetData(IDynamicDataProvider provider) {
            for (int i = 0; i < conditions.Length; i++) {
                if (conditions[i] is IDynamicDataHost host) host.OnSetData(provider);
            }
        }

        public void Arm(IConditionCallback callback) {
            _externalCallback = callback;

            for (int i = 0; i < conditions.Length; i++) {
                conditions[i].Arm(this);
            }
        }

        public void Disarm() {
            for (int i = 0; i < conditions.Length; i++) {
                conditions[i].Disarm();
            }

            _externalCallback = null;
        }

        public void OnConditionMatch(ICondition match) {
            if (IsMatched) _externalCallback?.OnConditionMatch(this);
        }

        public void OnFired() { }

        private bool CheckCondition() {
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
