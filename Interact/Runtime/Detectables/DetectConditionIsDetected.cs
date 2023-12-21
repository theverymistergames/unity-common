using System;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionIsDetected : IDetectCondition {

        public bool shouldBeDetected;

        public bool IsMatched((IDetector, IDetectable) context) {
            var (detector, detectable) = context;
            return shouldBeDetected == detectable.IsDetectedBy(detector);
        }
    }

}
