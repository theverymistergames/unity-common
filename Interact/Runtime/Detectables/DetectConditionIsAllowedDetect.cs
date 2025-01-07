using System;
using MisterGames.Common.Data;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionIsAllowedDetect : IDetectCondition {

        public Optional<bool> shouldBeAllowedToStartDetection;
        public Optional<bool> shouldBeAllowedToContinueDetection;

        public bool IsMatch((IDetector, IDetectable) context, float startTime) {
            var (detector, detectable) = context;
            return shouldBeAllowedToStartDetection.IsEmptyOrEquals(detectable.IsAllowedToStartDetectBy(detector)) &&
                   shouldBeAllowedToContinueDetection.IsEmptyOrEquals(detectable.IsAllowedToContinueDetectBy(detector));
        }
    }

}
