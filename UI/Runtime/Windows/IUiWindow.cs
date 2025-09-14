using UnityEngine;

namespace MisterGames.UI.Windows {
    
    public interface IUiWindow {
        
        GameObject GameObject { get; }
        int Layer { get; }
        bool IsRoot { get; }
        
        UiWindowState State { get; }
        bool IsFocused { get; }
        GameObject CurrentSelectable { get; }

        void NotifyWindowState(UiWindowState state, bool focused);
    }
    
}