using UnityEngine.UI;

namespace MisterGames.UI.Windows {
    
    public interface IUiWindow {
        
        Selectable FirstSelectable { get; }
        UiWindowState State { get; }
        bool IsRoot { get; }
        
        void NotifyWindowState(UiWindowState state);
    }
    
}