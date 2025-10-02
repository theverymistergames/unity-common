using System;
using UnityEngine;

namespace MisterGames.UI.Windows {
    
    public interface IUiWindowService {

        event Action OnWindowsHierarchyChanged;
        
        void RegisterWindow(IUiWindow window, UiWindowState state);
        void UnregisterWindow(IUiWindow window);
        
        IUiWindow GetFocusedWindow();
        IUiWindow GetFocusedWindow(int layer);
        IUiWindow GetFrontOpenedWindow();
        IUiWindow GetFrontOpenedWindow(int layer);
        
        IUiWindow GetParentWindow(IUiWindow child);
        IUiWindow GetRootWindow(IUiWindow window);
        IUiWindow FindClosestParentWindow(GameObject gameObject, bool includeSelf = true);
        bool IsChildWindow(IUiWindow window, IUiWindow child);

        bool HasOpenedWindows();
        bool HasOpenedWindows(out int topLayer);
        bool HasFocusedWindows();
        bool HasFocusedWindows(out int topLayer);
        bool IsInOpenedBranch(IUiWindow window);

        bool IsCursorRequired();
        
        bool SetWindowState(IUiWindow window, UiWindowState state);
        UiWindowState GetWindowState(IUiWindow window);

        void NotifyWindowEnabled(IUiWindow window, bool enabled);
    }
    
}