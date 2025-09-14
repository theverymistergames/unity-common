using System;
using UnityEngine;

namespace MisterGames.UI.Windows {
    
    public interface IUiWindowService {

        event Action OnWindowsHierarchyChanged;
        
        void RegisterWindow(IUiWindow window);
        void UnregisterWindow(IUiWindow window);
        
        void RegisterRelation(IUiWindow parent, IUiWindow child, UiWindowMode mode);
        void UnregisterRelation(IUiWindow parent, IUiWindow child);

        IUiWindow GetFrontWindow();
        IUiWindow GetParentWindow(IUiWindow child);
        IUiWindow GetClosestParentWindow(GameObject gameObject);
        
        bool HasOpenedWindows();
        void SetWindowState(IUiWindow window, UiWindowState state);
    }
    
}