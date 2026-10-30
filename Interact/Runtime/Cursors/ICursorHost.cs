namespace MisterGames.Interact.Cursors {

    public interface ICursorHost {
        void ApplyCursorIconOverride(object source, CursorIcon icon, int priority = 0);
        void ResetCursorIconOverride(object source);
    }

}
