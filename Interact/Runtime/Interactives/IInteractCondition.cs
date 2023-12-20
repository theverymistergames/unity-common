namespace MisterGames.Interact.Interactives {

    public interface IInteractCondition {
        bool IsMatch(IInteractiveUser user, IInteractive interactive);
    }

}
