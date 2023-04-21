using System;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConstraintIsDetected : IDetectionConstraint {

        public bool isDetected;

        public bool IsAllowedDetection(IDetector detector, IDetectable detectable) {
            return isDetected == detectable.IsDetectedBy(detector);
        }
    }

}
