using System;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConstraintMaxRadius : IDetectionConstraint {

        [Min(0f)] public float maxRadius;

        public bool IsAllowedDetection(IDetector detector, IDetectable detectable) {
            return Vector3.SqrMagnitude(detector.Transform.position - detectable.Transform.position) <= maxRadius * maxRadius;
        }
    }

}
