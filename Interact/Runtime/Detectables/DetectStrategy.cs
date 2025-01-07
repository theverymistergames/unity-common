using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [CreateAssetMenu(fileName = nameof(DetectStrategy), menuName = "MisterGames/Interactives/" + nameof(DetectStrategy))]
    public sealed class DetectStrategy : ScriptableObject {

        [SerializeReference] [SubclassSelector] private IDetectCondition _startConstraint;
        [SerializeReference] [SubclassSelector] private IDetectCondition _continueConstraint;

        public bool IsAllowedToStartDetection(IDetector detector, IDetectable detectable, float startTime) {
            return _startConstraint == null || _startConstraint.IsMatch((detector, detectable), startTime);
        }

        public bool IsAllowedToContinueDetection(IDetector detector, IDetectable detectable, float startTime) {
            return _continueConstraint == null || _continueConstraint.IsMatch((detector, detectable), startTime);
        }
    }

}
