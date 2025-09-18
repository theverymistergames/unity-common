using System;
using MisterGames.UI.Windows;
using UnityEngine;

namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationService {

        event Action<GameObject, IUiWindow> onSelectedGameObjectChanged;
        
        GameObject SelectedGameObject { get; }
        bool HasSelectedGameObject { get; }
        
        void SelectGameObject(GameObject gameObject);
        
        void PerformCancel();

        void AddNavigationCallback(IUiWindow window, IUiNavigationCallback callback);
        void RemoveNavigationCallback(IUiWindow window, IUiNavigationCallback callback);
    }
    
}