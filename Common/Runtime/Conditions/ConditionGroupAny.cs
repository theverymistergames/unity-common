using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Conditions {
    
    [Serializable]
    public sealed class ConditionGroupAny : ICondition, IConditionCallback {

        [SerializeReference] [SubclassSelector] public ICondition[] conditions;

        public bool IsMatched { get; private set; }

        private IConditionCallback _externalCallback;
        
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
