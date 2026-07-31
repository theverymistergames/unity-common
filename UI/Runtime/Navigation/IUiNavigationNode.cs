using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationNode {
        
        GameObject GameObject { get; }
        Selectable CurrentSelected { get; }
        UiNavigateFromOuterNodesOptions IncomeOuterNavigation { get; }
        bool IsScrollable { get; }
        RectTransform Viewport { get; }
        
        void Bind(Selectable selectable, UiNavigationMask mask = ~UiNavigationMask.None, UiNavigationOptions options = UiNavigationOptions.None);
        void Unbind(Selectable selectable);
        
        void UpdateNavigation();
        void OnNavigateOut(Selectable fromSelectable, UiNavigationDirection direction);
        
        void SetCurrentSelected(Selectable selectable);
    }
    
}