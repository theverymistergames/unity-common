using System;
using System.Collections.Generic;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationService {

        event Action<Selectable, IUiWindow> OnSelectableChanged;
        event Action OnNavigationHierarchyChanged;
        
        bool HasSelectedGameObject { get; }
        Selectable CurrentSelectable { get; }
        Selectable LastNonNullSelectable { get; }
        UiNavigationMask SelectedObjectNavigationMask { get; }
        UiNavigationOptions SelectedObjectOptions { get; }
        GameObject SelectedGameObject { get; }
        IUiWindow SelectedGameObjectWindow { get; }
        
        IReadOnlyCollection<Selectable> Selectables { get; }
        IReadOnlyCollection<IUiNavigationNode> Nodes { get; }
        IReadOnlyCollection<RectTransform> ScrollableViewports { get; }
        
        void SetCurrentSelectable(Selectable selectable);
        
        bool IsExitToPauseBlocked();
        void BlockExitToPause(object source);
        void UnblockExitToPause(object source);
        
        void NavigateOutTo(Selectable selectable, UiNavigationDirection direction);
        
        void NavigateBack();
        bool NavigateBackPerformedThisFrame();
        
        void BindNavigation(IUiNavigationNode node);
        void UnbindNavigation(IUiNavigationNode node);

        void BindNavigation(Selectable selectable, UiNavigationMask mask = ~UiNavigationMask.None, UiNavigationOptions options = default);
        void UnbindNavigation(Selectable selectable);

        IUiNavigationNode GetNavigationNode(GameObject gameObject);
        IUiNavigationNode GetParentNavigationNode(GameObject gameObject);
        IUiNavigationNode GetParentNavigationNode(Selectable selectable);
        IUiNavigationNode GetParentNavigationNode(IUiNavigationNode node);
        IUiNavigationNode FindClosestParentNavigationNode(GameObject gameObject, bool includeSelf = true);
        
        bool IsChildNode(IUiNavigationNode node, IUiNavigationNode parent, bool direct);
    }
    
}