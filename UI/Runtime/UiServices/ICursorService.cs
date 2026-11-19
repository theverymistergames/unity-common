namespace MisterGames.UI.UiServices {
    
    public interface ICursorService {

        void SetCursorIcon(object source, CursorIcon cursorIcon, int priority = 0);

        void ResetCursorIcon(object source);

        void BlockCursor(object source, bool block);
    }
    
}