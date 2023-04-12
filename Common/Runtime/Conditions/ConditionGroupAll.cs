using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Conditions {

    [Serializable]
    public sealed class ConditionGroupAll : ICondition, IConditionCallback, IDynamicDataHost {

        [SerializeReference] [SubclassSelector] public ICondition[] conditions;

        public bool IsMatched => CheckCondition();

        private IConditionCallback _externalCallback;
        private bool _isArmed;

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
            if (_isArmed) return;

            _externalCallback = callback;

            for (int i = 0; i < conditions.Length; i++) {
                conditions[i].Arm(this);
            }

            _isArmed = true;

            if (IsMatched) _externalCallback?.OnConditionMatch();
        }

        public void Disarm() {
            if (!_isArmed) return;

            for (int i = 0; i < conditions.Length; i++) {
                conditions[i].Disarm();
            }

            _isArmed = false;
        }

        public void OnConditionMatch() {
            if (IsMatched) _externalCallback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            for (int i = 0; i < conditions.Length; i++) {
                if (!conditions[i].IsMatched) return false;
            }

            return true;
        }
    }

}
