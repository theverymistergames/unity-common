using MisterGames.UI.Data;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public interface IUiWindow {
        
        Selectable FirstSelectable { get; }
        UiWindowState State { get; }
        bool IsRoot { get; }
        
        void NotifyWindowState(UiWindowState state);
    }
    
}