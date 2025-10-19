using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationNode {
        
        GameObject GameObject { get; }
        GameObject CurrentSelected { get; }
        UiNavigateFromOuterNodesOptions NavigateFromOuterNodesOptions { get; }
        bool IsScrollable { get; }
        RectTransform Viewport { get; }
        
        void Bind(Selectable selectable, UiNavigationMask mask = ~UiNavigationMask.None);
        void Unbind(Selectable selectable);

        void UpdateNavigation();
        
        void OnNavigateOut(Selectable fromSelectable, UiNavigationDirection direction);
    }
    
}