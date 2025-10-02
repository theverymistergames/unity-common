namespace MisterGames.UI.Service {
    
    public interface ICursorService {

        void UpdateCursorVisibility();
        void BlockCursor(object source, bool block);
    }
    
}