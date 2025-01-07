using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    [CreateAssetMenu(fileName = nameof(InteractionStrategy), menuName = "MisterGames/Interactives/" + nameof(InteractionStrategy))]
    public sealed class InteractionStrategy : ScriptableObject {

        [SerializeField] private bool _allowStopImmediatelyAfterStart;
        [SerializeReference] [SubclassSelector] private IInteractCondition _readyConstraint;
        [SerializeReference] [SubclassSelector] private IInteractCondition _startConstraint;
        [SerializeReference] [SubclassSelector] private IInteractCondition _continueConstraint;

        public bool IsReadyToStartInteraction(IInteractiveUser user, IInteractive interactive, float startTime) {
            return _readyConstraint == null || _readyConstraint.IsMatch((user, interactive), startTime);
        }

        public bool IsAllowedToStartInteraction(IInteractiveUser user, IInteractive interactive, float startTime) {
            return _startConstraint == null || _startConstraint.IsMatch((user, interactive), startTime);
        }

        public bool IsAllowedToContinueInteraction(IInteractiveUser user, IInteractive interactive, float startTime) {
            if (_allowStopImmediatelyAfterStart) {
                return _continueConstraint == null || _continueConstraint.IsMatch((user, interactive), startTime);
            }

            if (interactive.TryGetInteractionStartTime(user, out int startFrame) &&
                startFrame >= TimeSources.frameCount
            ) {
                return true;
            }

            return _continueConstraint == null || _continueConstraint.IsMatch((user, interactive), startTime);
        }
    }

}
