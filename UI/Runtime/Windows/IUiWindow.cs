using UnityEngine;

namespace MisterGames.UI.Windows {
    
    public interface IUiWindow {
        
        GameObject GameObject { get; }
        GameObject CurrentSelected { get; }
        
        int Layer { get; }
        bool IsRoot { get; }
        UiWindowMode Mode { get; }
        UiWindowState State { get; }
        bool IsFocused { get; }

        void NotifyWindowState(UiWindowState state, bool focused);
    }
    
}