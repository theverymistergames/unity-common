namespace MisterGames.Interact.Core {

    public interface IInteractiveStrategy {
        void UpdateInteractionState(IInteractiveUser user, IInteractive interactive);
    }

}
