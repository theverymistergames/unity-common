namespace MisterGames.Interact.Interactives {

    public interface IInteractionConstraint {
        bool IsSatisfied(IInteractiveUser user, IInteractive interactive);
    }

}
