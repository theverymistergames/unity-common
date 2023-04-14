namespace MisterGames.Interact.Cursors {

    public interface IInteractiveCursorHost {
        void StartOverrideCursorIcon(object source, CursorIcon icon);
        void StopOverrideCursorIcon(object source, CursorIcon icon);
    }

}
