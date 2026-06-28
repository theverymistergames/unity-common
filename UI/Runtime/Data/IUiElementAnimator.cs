using System;

namespace MisterGames.UI.Data {
    
    public interface IUiElementAnimator {
        
        event Action<UiElementState> OnStateChanged;
        UiElementState CurrentState { get; }
        
        void ApplyCustomState(UiElementState state);
        void ResetCustomState();
        
        void AnimateState(UiElementState state);
    }
    
}