using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Conditions {
    
    [Serializable]
    public sealed class ConditionGroupAny : ICondition, IConditionCallback, IDynamicDataHost {

        [SerializeReference] [SubclassSelector] public ICondition[] conditions;

        public bool IsMatched { get; private set; }

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
        }
        
        public void OnConditionMatch() {
            UpdateIsMatched();
            
            if (IsMatched) _externalCallback.OnConditionMatch();
        }

        private void UpdateIsMatched() {
            bool isMatched = false;

            for (int i = 0; i < conditions.Length; i++) {
                if (!conditions[i].IsMatched) continue;

                isMatched = true;
                break;
            }

            IsMatched = isMatched;
        }
    }
    
}
