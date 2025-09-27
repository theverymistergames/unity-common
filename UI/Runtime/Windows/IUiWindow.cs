using UnityEngine;

namespace MisterGames.UI.Windows {
    
    public interface IUiWindow {
        
        GameObject GameObject { get; }
        GameObject CurrentSelected { get; }
        
        int Layer { get; }
        UiWindowOpenMode OpenMode { get; }
        UiWindowCloseMode CloseMode { get; }
        UiWindowState State { get; }
        
        void NotifyWindowState(UiWindowState state);
    }
    
}