using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionGroup : IDetectCondition {

        public Mode mode;
        [SerializeReference] [SubclassSelector] private IDetectCondition[] conditions;

        public enum Mode {
            All,
            Any
        }

        public bool IsMatch(IDetector detector, IDetectable detectable) {
            switch (mode) {
                case Mode.All:
                    for (int i = 0; i < conditions.Length; i++) {
                        if (!conditions[i].IsMatch(detector, detectable)) return false;
                    }
                    return true;

                case Mode.Any:
                    for (int i = 0; i < conditions.Length; i++) {
                        if (conditions[i].IsMatch(detector, detectable)) return true;
                    }
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
