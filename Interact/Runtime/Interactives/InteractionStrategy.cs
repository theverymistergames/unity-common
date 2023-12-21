using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [CreateAssetMenu(fileName = nameof(InteractionStrategy), menuName = "MisterGames/Interactives/" + nameof(InteractionStrategy))]
    public sealed class InteractionStrategy : ScriptableObject {

        [SerializeField] private bool _allowStopImmediatelyAfterStart;
        [SerializeReference] [SubclassSelector] private IInteractCondition _readyConstraint;
        [SerializeReference] [SubclassSelector] private IInteractCondition _startConstraint;
        [SerializeReference] [SubclassSelector] private IInteractCondition _continueConstraint;

        public bool IsReadyToStartInteraction(IInteractiveUser user, IInteractive interactive) {
            return _readyConstraint == null || _readyConstraint.IsMatched((user, interactive));
        }

        public bool IsAllowedToStartInteraction(IInteractiveUser user, IInteractive interactive) {
            return _startConstraint == null || _startConstraint.IsMatched((user, interactive));
        }

        public bool IsAllowedToContinueInteraction(IInteractiveUser user, IInteractive interactive) {
            if (_allowStopImmediatelyAfterStart) {
                return _continueConstraint == null || _continueConstraint.IsMatched((user, interactive));
            }

            if (interactive.TryGetInteractionStartTime(user, out int startTime) &&
                startTime >= TimeSources.frameCount
            ) {
                return true;
            }

            return _continueConstraint == null || _continueConstraint.IsMatched((user, interactive));
        }
    }

}
