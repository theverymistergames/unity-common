using MisterGames.Common.Attributes;
using MisterGames.Interact.Core;
using UnityEngine;

namespace MisterGames.Interact.Strategy {

    [CreateAssetMenu(fileName = nameof(InteractiveStrategy), menuName = "MisterGames/Interactives/" + nameof(InteractiveStrategy))]
    public sealed class InteractiveStrategy : ScriptableObject, IInteractiveStrategy {

        [SerializeReference] [SubclassSelector] private IInteractiveStrategy _strategy;

        public void UpdateInteractionState(IInteractiveUser user, IInteractive interactive) {
            _strategy.UpdateInteractionState(user, interactive);
        }
    }

}
