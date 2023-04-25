using System;
using MisterGames.Common.Data;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConstraintIsAllowedDetection : IDetectionConstraint {

        public Optional<bool> shouldBeAllowedToStartDetection;
        public Optional<bool> shouldBeAllowedToContinueDetection;

        public bool IsAllowedDetection(IDetector detector, IDetectable detectable) {
            return shouldBeAllowedToStartDetection.IsEmptyOrEquals(detectable.IsAllowedToStartDetectBy(detector)) &&
                   shouldBeAllowedToContinueDetection.IsEmptyOrEquals(detectable.IsAllowedToContinueDetectBy(detector));
        }
    }

}
