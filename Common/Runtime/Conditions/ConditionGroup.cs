using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Conditions {

    [SubclassSelectorIgnore]
    [Serializable]
    public class ConditionGroup<T, C> : ICondition<T> where C : ICondition<T> {

        public Mode mode;
        [SerializeReference] [SubclassSelector] public C[] conditions;

        public enum Mode {
            All,
            Any
        }

        public bool IsMatched(T context) {
            switch (mode) {
                case Mode.All:
                    for (int i = 0; i < conditions.Length; i++) {
                        if (!conditions[i].IsMatched(context)) return false;
                    }
                    return true;

                case Mode.Any:
                    for (int i = 0; i < conditions.Length; i++) {
                        if (conditions[i].IsMatched(context)) return true;
                    }
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
