using System;
using MisterGames.UI.Components;
using MisterGames.UI.Data;

namespace MisterGames.UI.Service {
    
    public interface IUiWindowService {

        event Action OnWindowsHierarchyChanged;
        
        void RegisterWindow(IUiWindow window, int layer);
        void UnregisterWindow(IUiWindow window);
        
        void RegisterRelation(IUiWindow parent, IUiWindow child, UiWindowMode mode);
        void UnregisterRelation(IUiWindow parent, IUiWindow child);

        IUiWindow GetFrontWindow();
        IUiWindow GetParentWindow(IUiWindow child);
        
        bool HasOpenedWindows();
        void SetWindowState(IUiWindow window, UiWindowState state);
    }
    
}