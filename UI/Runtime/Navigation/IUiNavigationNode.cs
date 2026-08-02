using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationNode {
        
        GameObject GameObject { get; }
        Selectable CurrentSelectable { get; }
        Selectable DefaultSelectable { get; }
        UiIncomingOuterNavigationOptions IncomingOuterNavigation { get; }
        bool IsScrollable { get; }
        RectTransform Viewport { get; }
        
        void Bind(Selectable selectable, UiNavigationMask mask = ~UiNavigationMask.None, UiNavigationOptions options = UiNavigationOptions.None);
        void Unbind(Selectable selectable);
        
        void UpdateNavigation();
        void OnNavigateOut(Selectable fromSelectable, UiNavigationDirection direction);
        
        void SetCurrentSelectable(Selectable selectable);
    }
    
}