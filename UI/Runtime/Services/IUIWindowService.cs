using System;

namespace MisterGames.UI.Services {
    
    public interface IUIWindowService {

        event Action OnWindowsChanged;
        
        bool HasOpenedWindows();
        
        void NotifyOpenedWindow(object source, bool opened);
        
    }
    
}