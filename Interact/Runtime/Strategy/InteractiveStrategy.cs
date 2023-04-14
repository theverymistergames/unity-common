using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Interact.Core {

    [CreateAssetMenu(fileName = nameof(InteractiveStrategy), menuName = "MisterGames/Interactives/" + nameof(InteractiveStrategy))]
    public sealed class InteractiveStrategy : ScriptableObject, IInteractiveStrategy {

        [SerializeReference] [SubclassSelector] private IInteractiveStrategy _strategy;

        public void UpdateInteractionState(IInteractiveUser user, IInteractive interactive) {
            _strategy.UpdateInteractionState(user, interactive);
        }
    }

}
