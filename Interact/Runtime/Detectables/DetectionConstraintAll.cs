using System;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectionConstraintAll : IDetectionConstraint {

        [SerializeReference] [SubclassSelector] public IDetectionConstraint[] constraints;

        public bool IsAllowedDetection(IDetector detector, IDetectable detectable) {
            for (int i = 0; i < constraints.Length; i++) {
                if (!constraints[i].IsAllowedDetection(detector, detectable)) return false;
            }

            return true;
        }
    }

}
