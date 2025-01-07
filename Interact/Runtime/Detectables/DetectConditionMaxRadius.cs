using System;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionMaxRadius : IDetectCondition {

        [Min(0f)] public float maxRadius;

        public bool IsMatch((IDetector, IDetectable) context, float startTime) {
            var (detector, detectable) = context;
            return (detector.Transform.position - detectable.Transform.position).sqrMagnitude <= maxRadius * maxRadius;
        }
    }

}
