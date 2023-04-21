namespace MisterGames.Interact.Interactives {

    public interface IInteractionConstraint {
        bool IsAllowedInteraction(IInteractiveUser user, IInteractive interactive);
    }

}
