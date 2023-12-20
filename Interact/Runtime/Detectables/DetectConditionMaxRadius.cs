﻿using System;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionMaxRadius : IDetectCondition {

        [Min(0f)] public float maxRadius;

        public bool IsMatch(IDetector detector, IDetectable detectable) {
            return (detector.Transform.position - detectable.Transform.position).sqrMagnitude <= maxRadius * maxRadius;
        }
    }

}
