using System;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionIsDetected : IDetectCondition {

        public bool shouldBeDetected;

        public bool IsMatch(IDetector detector, IDetectable detectable) {
            return shouldBeDetected == detectable.IsDetectedBy(detector);
        }
    }

}
