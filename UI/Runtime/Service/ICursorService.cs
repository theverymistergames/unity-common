namespace MisterGames.UI.UiServices {
    
    public interface ICursorService {

        void UpdateCursorVisibility();
        void BlockCursor(object source, bool block);
    }
    
}