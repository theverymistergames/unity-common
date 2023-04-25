using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [CreateAssetMenu(fileName = nameof(InteractionStrategy), menuName = "MisterGames/Interactives/" + nameof(InteractionStrategy))]
    public sealed class InteractionStrategy : ScriptableObject {

        [SerializeField] private bool _allowStopImmediatelyAfterStart;
        [SerializeReference] [SubclassSelector] private IInteractionConstraint _readyConstraint;
        [SerializeReference] [SubclassSelector] private IInteractionConstraint _startConstraint;
        [SerializeReference] [SubclassSelector] private IInteractionConstraint _continueConstraint;

        public bool IsReadyToStartInteraction(IInteractiveUser user, IInteractive interactive) {
            return _readyConstraint != null && _readyConstraint.IsSatisfied(user, interactive);
        }

        public bool IsAllowedToStartInteraction(IInteractiveUser user, IInteractive interactive) {
            return _startConstraint != null && _startConstraint.IsSatisfied(user, interactive);
        }

        public bool IsAllowedToContinueInteraction(IInteractiveUser user, IInteractive interactive) {
            if (_allowStopImmediatelyAfterStart) {
                return _continueConstraint != null && _continueConstraint.IsSatisfied(user, interactive);
            }

            if (interactive.TryGetInteractionStartTime(user, out int startTime) && startTime >= TimeSources.FrameCount) {
                return true;
            }

            return _continueConstraint != null && _continueConstraint.IsSatisfied(user, interactive);
        }
    }

}
