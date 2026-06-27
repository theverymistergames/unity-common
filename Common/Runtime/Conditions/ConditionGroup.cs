using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Conditions {

    [SubclassSelectorIgnore]
    [Serializable]
    public class ConditionGroup<TCondition, TContext> : ICondition<TContext>
        where TCondition : ICondition<TContext>
    {
        public Mode mode;
        [SerializeReference] [SubclassSelector] public TCondition[] conditions;

        public enum Mode {
            All,
            Any
        }

        public bool IsMatch(TContext context) {
            switch (mode) {
                case Mode.All:
                    for (int i = 0; i < conditions.Length; i++) {
                        if (!conditions[i].IsMatch(context)) return false;
                    }
                    return true;

                case Mode.Any:
                    for (int i = 0; i < conditions.Length; i++) {
                        if (conditions[i].IsMatch(context)) return true;
                    }
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
