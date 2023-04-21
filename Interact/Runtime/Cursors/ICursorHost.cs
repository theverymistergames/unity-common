namespace MisterGames.Interact.Cursors {

    public interface ICursorHost {
        void Register(object source);
        void Unregister(object source);

        void ApplyCursorIcon(object source, CursorIcon icon);
    }

}
