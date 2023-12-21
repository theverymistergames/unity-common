using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [CreateAssetMenu(fileName = nameof(DetectStrategy), menuName = "MisterGames/Interactives/" + nameof(DetectStrategy))]
    public sealed class DetectStrategy : ScriptableObject {

        [SerializeReference] [SubclassSelector] private IDetectCondition _startConstraint;
        [SerializeReference] [SubclassSelector] private IDetectCondition _continueConstraint;

        public bool IsAllowedToStartDetection(IDetector detector, IDetectable detectable) {
            return _startConstraint == null || _startConstraint.IsMatched((detector, detectable));
        }

        public bool IsAllowedToContinueDetection(IDetector detector, IDetectable detectable) {
            return _continueConstraint == null || _continueConstraint.IsMatched((detector, detectable));
        }
    }

}
