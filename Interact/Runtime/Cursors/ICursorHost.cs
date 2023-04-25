namespace MisterGames.Interact.Cursors {

    public interface ICursorHost {
        void ApplyCursorIconOverride(object source, CursorIcon icon);
        void ResetCursorIconOverride(object source);
    }

}
