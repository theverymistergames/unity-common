using System;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConstraintInDirectView : IDetectionConstraint {

        public bool shouldBeInDirectView;

        public bool IsAllowedDetection(IDetector detector, IDetectable detectable) {
            return shouldBeInDirectView == detector.IsInDirectView(detectable, out _);
        }
    }

}
