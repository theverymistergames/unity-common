using System;

namespace MisterGames.UI.Data {
    
    public interface IUiElementAnimator {
        
        event Action<UiElementState> OnStateChanged;
        UiElementState CurrentState { get; }
        
        void AnimateState(UiElementState state);
        void SetBlockedState(bool blocked);
    }
    
}