using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public interface IUiNavigationNode {
        
        GameObject GameObject { get; }

        void Bind(Selectable selectable);
        void Unbind(Selectable selectable);
        
        void Bind(IUiNavigationNode node);
        void Unbind(IUiNavigationNode node);
    }
    
}