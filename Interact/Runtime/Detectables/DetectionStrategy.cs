using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [CreateAssetMenu(fileName = nameof(DetectionStrategy), menuName = "MisterGames/Interactives/" + nameof(DetectionStrategy))]
    public sealed class DetectionStrategy : ScriptableObject {

        [SerializeReference] [SubclassSelector] private IDetectionConstraint _startConstraint;
        [SerializeReference] [SubclassSelector] private IDetectionConstraint _continueConstraint;

        public bool IsAllowedToStartDetection(IDetector detector, IDetectable detectable) {
            return _startConstraint == null || _startConstraint.IsAllowedDetection(detector, detectable);
        }

        public bool IsAllowedToContinueDetection(IDetector detector, IDetectable detectable) {
            return _continueConstraint == null || _continueConstraint.IsAllowedDetection(detector, detectable);
        }
    }

}
