using MisterGames.UI.Navigation;
using UnityEngine;

namespace MisterGames.UI.Windows {
    
    public interface IUiWindow {
        
        GameObject GameObject { get; }
        int Layer { get; }
        UiWindowOpenMode OpenMode { get; }
        UiWindowCloseMode CloseMode { get; }
        UiWindowState State { get; }
        UiWindowOptions Options { get; }
        IUiNavigationNode Node { get; }
        
        void NotifyWindowState(UiWindowState state);
    }
    
}