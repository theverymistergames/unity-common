using System;

namespace MisterGames.UI.Data {
    
    public interface IUiElementAnimator {
        
        event Action<UiElementState> OnStateChanged;
        UiElementState CurrentState { get; }
        
        void AnimateState(UiElementState state);
        void SetForceState(UiElementState state);
        void ResetForceState();
        void SetBlockedState(bool blocked);
    }
    
}