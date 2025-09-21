using System;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationService {

        event Action<GameObject, IUiWindow> onSelectedGameObjectChanged;
        
        GameObject SelectedGameObject { get; }
        bool HasSelectedGameObject { get; }
        
        void SelectGameObject(GameObject gameObject);
        
        void PerformCancel();

        void AddNavigationCallback(IUiWindow window, IUiNavigationCallback callback);
        void RemoveNavigationCallback(IUiWindow window, IUiNavigationCallback callback);

        void BindNavigation(IUiNavigationNode node);
        void UnbindNavigation(IUiNavigationNode node);

        void BindNavigation(Selectable selectable);
        void UnbindNavigation(Selectable selectable);
    }
    
}