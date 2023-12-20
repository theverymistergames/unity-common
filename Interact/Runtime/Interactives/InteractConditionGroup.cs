using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [Serializable]
    public sealed class InteractConditionGroup : IInteractCondition {

        public Mode mode;
        [SerializeReference] [SubclassSelector] public IInteractCondition[] conditions;

        public enum Mode {
            All,
            Any
        }

        public bool IsMatch(IInteractiveUser user, IInteractive interactive) {
            switch (mode) {
                case Mode.All:
                    for (int i = 0; i < conditions.Length; i++) {
                        if (!conditions[i].IsMatch(user, interactive)) return false;
                    }
                    return true;

                case Mode.Any:
                    for (int i = 0; i < conditions.Length; i++) {
                        if (conditions[i].IsMatch(user, interactive)) return true;
                    }
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
