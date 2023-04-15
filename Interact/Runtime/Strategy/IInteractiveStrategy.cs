using MisterGames.Interact.Core;

namespace MisterGames.Interact.Strategy {

    public interface IInteractiveStrategy {
        void UpdateInteractionState(IInteractiveUser user, IInteractive interactive);
    }

}
