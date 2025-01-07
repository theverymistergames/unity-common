using System;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionIsDetected : IDetectCondition {

        public bool shouldBeDetected;

        public bool IsMatch((IDetector, IDetectable) context, float startTime) {
            var (detector, detectable) = context;
            return shouldBeDetected == detectable.IsDetectedBy(detector);
        }
    }

}
