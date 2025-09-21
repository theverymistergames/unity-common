using System;
using UnityEngine;

namespace MisterGames.UI.Windows {
    
    public interface IUiWindowService {

        event Action OnWindowsHierarchyChanged;
        
        void RegisterWindow(IUiWindow window);
        void UnregisterWindow(IUiWindow window);
        
        IUiWindow GetFocusedWindow();
        IUiWindow GetParentWindow(IUiWindow child);
        IUiWindow FindClosestParentWindow(GameObject gameObject, bool includeSelf = true);
        bool IsChildWindow(IUiWindow window, IUiWindow child);

        bool HasOpenedWindows();
        void SetWindowState(IUiWindow window, UiWindowState state);
    }
    
}