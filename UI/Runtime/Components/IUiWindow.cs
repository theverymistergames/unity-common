using MisterGames.UI.Data;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public interface IUiWindow {
        
        Selectable FirstSelected { get; }
        UiWindowState State { get; }
        bool IsRoot { get; }
        
        void NotifyWindowState(UiWindowState state);
    }
    
}