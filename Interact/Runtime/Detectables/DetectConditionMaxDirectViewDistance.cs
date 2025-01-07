using System;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionMaxDirectViewDistance : IDetectCondition {

        [Min(0f)] public float maxDistance;

        public bool IsMatch((IDetector, IDetectable) context, float startTime) {
            var (detector, detectable) = context;
            return detector.IsInDirectView(detectable, out float distance) && distance <= maxDistance;
        }
    }

}
