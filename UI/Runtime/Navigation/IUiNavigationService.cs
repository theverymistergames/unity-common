using System;
using System.Collections.Generic;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationService {

        event Action<GameObject, IUiWindow> OnSelectedGameObjectChanged;
        
        GameObject SelectedGameObject { get; }
        IUiWindow SelectedGameObjectWindow { get; }
        bool HasSelectedGameObject { get; }
        
        IReadOnlyCollection<Selectable> Selectables { get; }
        
        void SelectGameObject(GameObject gameObject);

        bool IsExitToPauseBlocked();
        void BlockExitToPause(object source);
        void UnblockExitToPause(object source);
        
        void PerformCancel();
        
        void BindNavigation(IUiNavigationNode node);
        void UnbindNavigation(IUiNavigationNode node);

        void BindNavigation(Selectable selectable);
        void UnbindNavigation(Selectable selectable);

        IUiNavigationNode GetParentNavigationNode(Selectable selectable);
        IUiNavigationNode GetParentNavigationNode(IUiNavigationNode node);
        IUiNavigationNode FindClosestParentNavigationNode(GameObject gameObject, bool includeSelf = true);
        
        bool IsChildNode(IUiNavigationNode node, IUiNavigationNode parent, bool direct);
    }
    
}