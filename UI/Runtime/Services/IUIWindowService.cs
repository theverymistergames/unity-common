using System;

namespace MisterGames.UI.Service {
    
    public interface IUIWindowService {

        event Action OnWindowsChanged;
        
        bool HasOpenedWindows();
        
        void NotifyOpenedWindow(object source, bool opened);
        
    }
    
}