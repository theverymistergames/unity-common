using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Windows {
    
    public interface IUiWindow {
        
        GameObject GameObject { get; }
        Selectable CurrentSelected { get; }
        Selectable FirstSelected { get; set; }
        
        int Layer { get; }
        UiWindowOpenMode OpenMode { get; }
        UiWindowCloseMode CloseMode { get; }
        UiWindowState State { get; }
        UiWindowOptions Options { get; }
        
        void NotifyWindowState(UiWindowState state);
    }
    
}