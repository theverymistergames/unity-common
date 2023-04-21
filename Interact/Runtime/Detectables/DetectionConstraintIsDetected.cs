using System;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConstraintIsDetected : IDetectionConstraint {

        public bool shouldBeDetected;

        public bool IsAllowedDetection(IDetector detector, IDetectable detectable) {
            return shouldBeDetected == detectable.IsDetectedBy(detector);
        }
    }

}
