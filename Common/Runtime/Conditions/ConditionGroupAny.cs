using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Conditions {
    
    [Serializable]
    public sealed class ConditionGroupAny : ICondition, IConditionCallback, IDynamicDataHost {

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

            if (IsMatched) _externalCallback?.OnConditionMatch();
        }

        public void Disarm() {
            for (int i = 0; i < conditions.Length; i++) {
                conditions[i].Disarm();
            }

            _externalCallback = null;
        }
        
        public void OnConditionMatch() {
            if (IsMatched) _externalCallback?.OnConditionMatch();
        }

        private bool CheckCondition() {
            for (int i = 0; i < conditions.Length; i++) {
                if (conditions[i].IsMatched) return true;
            }

            return false;
        }
    }
    
}
