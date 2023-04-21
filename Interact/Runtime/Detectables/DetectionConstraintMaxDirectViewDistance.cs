using System;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConstraintMaxDirectViewDistance : IDetectionConstraint {

        [Min(0f)] public float maxDistance;

        public bool IsAllowedDetection(IDetector detector, IDetectable detectable) {
            return detector.IsInDirectView(detectable, out float distance) && distance <= maxDistance * maxDistance;
        }
    }

}
