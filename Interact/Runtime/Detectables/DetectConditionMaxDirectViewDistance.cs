using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionMaxDirectViewDistance : IDetectCondition {

        [Min(0f)] public float maxDistance;

        public bool IsMatch((IDetector, IDetectable) context) {
            var (detector, detectable) = context;
            return detector.IsInDirectView(detectable, out float distance) && distance <= maxDistance;
        }
    }

}
