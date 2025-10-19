using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationNodeElement {
        
        Selectable Selectable { get; }
        
        bool CanMove(UiNavigationDirection direction);
    }
    
}